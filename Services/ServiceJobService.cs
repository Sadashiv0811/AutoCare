using System.Data;
using AutoCare.Api.Data;
using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Services;

public class ServiceJobService
{
    private readonly ServiceJobRepository _repository;
    private readonly AppDbContext _context;

    public ServiceJobService(ServiceJobRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<ServiceJobResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(MapToResponse).ToList();

    public async Task<List<ServiceJobResponseDto>> GetMyJobsAsync(int userId) =>
        (await _repository.GetByCustomerUserIdAsync(userId)).Select(MapToResponse).ToList();

    public async Task<ServiceJobResponseDto?> GetMyJobAsync(int id, int userId)
    {
        var job = await _repository.GetByIdAndCustomerUserIdAsync(id, userId);
        return job == null ? null : MapToResponse(job);
    }

    public async Task<List<ServiceJobResponseDto>> GetAssignedJobsAsync(int userId) =>
        (await _repository.GetByMechanicUserIdAsync(userId)).Select(MapToResponse).ToList();

    public async Task<ServiceJobResponseDto?> GetAssignedJobAsync(int id, int userId)
    {
        var job = await _repository.GetByIdAndMechanicUserIdAsync(id, userId);
        return job == null ? null : MapToResponse(job);
    }

    public async Task<ServiceJobResponseDto?> GetByIdAsync(int id)
    {
        var job = await _repository.GetByIdAsync(id);
        return job == null ? null : MapToResponse(job);
    }

    public async Task<ServiceJobResponseDto?> CreateAsync(ServiceJobCreateDto dto)
    {
        // New jobs must always begin in Scheduled. Only the assigned mechanic
        // may advance a job later through PATCH /{id}/status.
        if (dto.Mileage < 0 || !IsValidFutureSchedule(dto.ScheduledDate) ||
            dto.Status != ServiceJobStatus.Scheduled ||
            !Enum.IsDefined(typeof(ServiceJobStatus), dto.Status))
            return null;

        if (!await _repository.VehicleExistsAsync(dto.VehicleId) ||
            !await _repository.MechanicIsActiveAsync(dto.MechanicId) ||
            !await _repository.ServiceTypeIsActiveAsync(dto.ServiceTypeId))
            return null;

        var job = new ServiceJob
        {
            JobNumber = GenerateJobNumber(),
            VehicleId = dto.VehicleId,
            MechanicId = dto.MechanicId,
            ServiceTypeId = dto.ServiceTypeId,
            Mileage = dto.Mileage,
            ProblemDescription = dto.ProblemDescription?.Trim(),
            InspectionNotes = dto.InspectionNotes?.Trim(),
            Status = ServiceJobStatus.Scheduled,
            ScheduledDate = dto.ScheduledDate
        };

        var created = await _repository.CreateAsync(job);
        var details = await _repository.GetByIdAsync(created.Id);
        return details == null ? null : MapToResponse(details);
    }

    public async Task<ServiceJobResponseDto?> UpdateAsync(int id, ServiceJobUpdateDto dto)
    {
        if (dto.Mileage < 0 || dto.ScheduledDate == default)
            return null;

        var job = await _repository.GetTrackedByIdAsync(id);
        if (job == null) return null;

        // Status and lifecycle timestamps are deliberately absent from the update DTO.
        // This endpoint cannot change them, even if a client submits extra JSON fields.

        if (job.Status == ServiceJobStatus.Scheduled &&
            dto.ScheduledDate.Date < DateTime.UtcNow.Date)
            return null;

        if (!await _repository.VehicleExistsAsync(dto.VehicleId) ||
            !await _repository.MechanicIsActiveAsync(dto.MechanicId) ||
            !await _repository.ServiceTypeIsActiveAsync(dto.ServiceTypeId))
            return null;

        job.VehicleId = dto.VehicleId;
        job.MechanicId = dto.MechanicId;
        job.ServiceTypeId = dto.ServiceTypeId;
        job.Mileage = dto.Mileage;
        job.ProblemDescription = dto.ProblemDescription?.Trim();
        job.InspectionNotes = dto.InspectionNotes?.Trim();
        job.ScheduledDate = dto.ScheduledDate;
        await _repository.UpdateAsync(job);
        var updated = await _repository.GetByIdAsync(id);
        return updated == null ? null : MapToResponse(updated);
    }

    public async Task<ServiceJobResponseDto?> UpdateStatusAsync(
        int id, int mechanicUserId, ServiceJobStatus status)
    {
        if (!Enum.IsDefined(typeof(ServiceJobStatus), status))
            throw new InvalidOperationException("The requested service status is invalid.");

        // The status change and any maintenance record must be committed together.
        // Serializable isolation also protects the check against duplicate records
        // when status updates arrive close together.
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        var job = await _repository.GetTrackedByIdAndMechanicUserIdAsync(
            id, mechanicUserId);
        if (job == null) return null;

        var expectedNext = (int)job.Status + 1;
        if ((int)status != expectedNext)
            throw new InvalidOperationException(
                $"Invalid status transition from {job.Status} to {status}. Only the next sequential status is allowed.");

        var now = DateTime.UtcNow;
        job.Status = status;
        switch (status)
        {
            case ServiceJobStatus.Received:
                job.ReceivedDate = now;
                break;
            case ServiceJobStatus.Completed:
                job.CompletedDate = now;
                break;
            case ServiceJobStatus.Delivered:
                job.DeliveredDate = now;

                // Create one maintenance record for the delivered job. If a record
                // was already created manually for this job, keep it and avoid a
                // duplicate.
                var maintenanceRecordExists = await _context.MaintenanceRecords
                    .AnyAsync(record => record.ServiceJobId == job.Id);

                if (!maintenanceRecordExists)
                {
                    var notes = new List<string>();

                    if (!string.IsNullOrWhiteSpace(job.ProblemDescription))
                        notes.Add($"Reported issue: {job.ProblemDescription.Trim()}");

                    if (!string.IsNullOrWhiteSpace(job.InspectionNotes))
                        notes.Add($"Inspection: {job.InspectionNotes.Trim()}");

                    notes.AddRange(job.ServiceNotes
                        .Where(note => !string.IsNullOrWhiteSpace(note.Note))
                        .OrderBy(note => note.CreatedAt)
                        .Select(note => note.Note.Trim()));

                    var maintenanceNotes = string.Join(
                        Environment.NewLine, notes);

                    if (maintenanceNotes.Length > 1000)
                        maintenanceNotes = maintenanceNotes[..1000];

                    // Prefer the final invoiced amount. If an invoice has not yet
                    // been created, use the configured service price plus the
                    // recorded parts total instead of inventing a cost.
                    var maintenanceCost = job.Invoice?.TotalAmount
                        ?? (job.ServiceType.BasePrice +
                            job.ServiceJobParts.Sum(part => part.TotalPrice));

                    _context.MaintenanceRecords.Add(new MaintenanceRecord
                    {
                        VehicleId = job.VehicleId,
                        ServiceJobId = job.Id,
                        ServiceName = job.ServiceType.Name,
                        Mileage = job.Mileage,
                        Cost = maintenanceCost,
                        ServiceDate = now,
                        Notes = string.IsNullOrWhiteSpace(maintenanceNotes)
                            ? null
                            : maintenanceNotes
                    });
                }

                break;
        }

        // SaveChanges persists both the job status and maintenance record in the
        // same database transaction.
        await _repository.UpdateAsync(job);
        await transaction.CommitAsync();

        var updated = await _repository.GetByIdAsync(id);
        return updated == null ? null : MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var job = await _repository.GetTrackedByIdAsync(id);
        if (job == null) return false;

        if (await _repository.HasInvoiceAsync(id))
            throw new InvalidOperationException(
                "A service job with an invoice cannot be deleted. Preserve the financial history instead.");

        // Inventory restoration and deletion are saved together in one transaction.
        foreach (var assignment in job.ServiceJobParts)
        {
            if (assignment.Part != null)
                assignment.Part.Quantity = checked(assignment.Part.Quantity + assignment.Quantity);
        }

        await _repository.DeleteAsync(job);
        await transaction.CommitAsync();
        return true;
    }

    private static bool IsValidFutureSchedule(DateTime value) =>
        value != default && value.Date >= DateTime.UtcNow.Date;

    private static string GenerateJobNumber()
    {
        // The database unique index remains the final uniqueness guarantee.
        // A 64-bit random suffix avoids collisions for concurrent requests while
        // keeping the value under the JobNumber column's 30-character limit.
        var suffix = Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
        return $"JOB-{DateTime.UtcNow.Year}-{suffix}";
    }

    private static ServiceJobResponseDto MapToResponse(ServiceJob job) => new()
    {
        Id = job.Id,
        JobNumber = job.JobNumber,
        VehicleId = job.VehicleId,
        VehicleRegistrationNumber = job.Vehicle?.RegistrationNumber ?? string.Empty,
        MechanicId = job.MechanicId,
        MechanicName = job.Mechanic?.FullName ?? string.Empty,
        ServiceTypeId = job.ServiceTypeId,
        ServiceTypeName = job.ServiceType?.Name ?? string.Empty,
        Mileage = job.Mileage,
        ProblemDescription = job.ProblemDescription,
        InspectionNotes = job.InspectionNotes,
        Status = job.Status,
        ScheduledDate = job.ScheduledDate,
        ReceivedDate = job.ReceivedDate,
        CompletedDate = job.CompletedDate,
        DeliveredDate = job.DeliveredDate,
        CreatedAt = job.CreatedAt,
        PartCount = job.ServiceJobParts?.Count ?? 0,
        NoteCount = job.ServiceNotes?.Count ?? 0,
        HasInvoice = job.Invoice != null
    };
}
