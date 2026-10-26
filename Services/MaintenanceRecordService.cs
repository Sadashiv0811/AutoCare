using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class MaintenanceRecordService
{
    private readonly MaintenanceRecordRepository _repository;

    public MaintenanceRecordService(
        MaintenanceRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MaintenanceRecordResponseDto>> GetAllAsync()
    {
        var records = await _repository.GetAllAsync();

        return records
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<MaintenanceRecordResponseDto>> GetMyRecordsAsync(int userId)
    {
        var records = await _repository.GetByCustomerUserIdAsync(userId);
        return records.Select(MapToResponse).ToList();
    }

    public async Task<MaintenanceRecordResponseDto?> GetMyRecordAsync(int id, int userId)
    {
        var record = await _repository.GetByIdAndCustomerUserIdAsync(id, userId);
        return record == null ? null : MapToResponse(record);
    }

    public async Task<MaintenanceRecordResponseDto?> GetByIdAsync(
        int id)
    {
        var record = await _repository.GetByIdAsync(id);

        return record == null
            ? null
            : MapToResponse(record);
    }

    public async Task<MaintenanceRecordResponseDto?> CreateAsync(
        MaintenanceRecordCreateDto dto)
    {
        var vehicleExists =
            await _repository.VehicleExistsAsync(dto.VehicleId);

        if (!vehicleExists)
        {
            return null;
        }

        if (dto.ServiceJobId.HasValue)
        {
            var serviceJobExists =
                await _repository.ServiceJobExistsAsync(
                    dto.ServiceJobId.Value);

            if (!serviceJobExists)
            {
                return null;
            }
        }

        var record = new MaintenanceRecord
        {
            VehicleId = dto.VehicleId,

            ServiceJobId = dto.ServiceJobId,

            ServiceName = dto.ServiceName.Trim(),

            Mileage = dto.Mileage,

            Cost = dto.Cost,

            ServiceDate = dto.ServiceDate,

            Notes = dto.Notes?.Trim()
        };

        var createdRecord =
            await _repository.CreateAsync(record);

        var recordWithDetails =
            await _repository.GetByIdAsync(
                createdRecord.Id);

        return recordWithDetails == null
            ? null
            : MapToResponse(recordWithDetails);
    }

    public async Task<MaintenanceRecordResponseDto?> UpdateAsync(
        int id,
        MaintenanceRecordUpdateDto dto)
    {
        var record =
            await _repository.GetTrackedByIdAsync(id);

        if (record == null)
        {
            return null;
        }

        var vehicleExists =
            await _repository.VehicleExistsAsync(dto.VehicleId);

        if (!vehicleExists)
        {
            return null;
        }

        if (dto.ServiceJobId.HasValue)
        {
            var serviceJobExists =
                await _repository.ServiceJobExistsAsync(
                    dto.ServiceJobId.Value);

            if (!serviceJobExists)
            {
                return null;
            }
        }

        record.VehicleId =
            dto.VehicleId;

        record.ServiceJobId =
            dto.ServiceJobId;

        record.ServiceName =
            dto.ServiceName.Trim();

        record.Mileage =
            dto.Mileage;

        record.Cost =
            dto.Cost;

        record.ServiceDate =
            dto.ServiceDate;

        record.Notes =
            dto.Notes?.Trim();

        await _repository.UpdateAsync(record);

        var updatedRecord =
            await _repository.GetByIdAsync(id);

        return updatedRecord == null
            ? null
            : MapToResponse(updatedRecord);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var record =
            await _repository.GetTrackedByIdAsync(id);

        if (record == null)
        {
            return false;
        }

        await _repository.DeleteAsync(record);

        return true;
    }

    private static MaintenanceRecordResponseDto MapToResponse(
        MaintenanceRecord record)
    {
        return new MaintenanceRecordResponseDto
        {
            Id = record.Id,

            VehicleId = record.VehicleId,

            VehicleRegistrationNumber =
                record.Vehicle?.RegistrationNumber
                ?? string.Empty,

            ServiceJobId =
                record.ServiceJobId,

            JobNumber =
                record.ServiceJob?.JobNumber,

            ServiceName =
                record.ServiceName,

            Mileage =
                record.Mileage,

            Cost =
                record.Cost,

            ServiceDate =
                record.ServiceDate,

            Notes =
                record.Notes
        };
    }
}