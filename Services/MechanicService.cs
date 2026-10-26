using Microsoft.EntityFrameworkCore;
using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class MechanicService
{
    private readonly MechanicRepository _repository;

    public MechanicService(MechanicRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MechanicResponseDto>> GetAllAsync()
    {
        var mechanics = await _repository.GetAllAsync();

        return mechanics.Select(MapToResponse).ToList();
    }

    public async Task<MechanicResponseDto?> GetByIdAsync(int id)
    {
        var mechanic = await _repository.GetByIdAsync(id);

        return mechanic == null
            ? null
            : MapToResponse(mechanic);
    }

    public async Task<(MechanicResponseDto? Mechanic, string? Error)> CreateAsync(
        MechanicCreateDto dto)
    {
        var user = await _repository.GetMechanicUserAsync(dto.UserId);

        if (user == null)
        {
            return (
                null,
                "User not found or user does not have the Mechanic role."
            );
        }

        var profileExists =
            await _repository.MechanicExistsForUserAsync(dto.UserId);

        if (profileExists)
        {
            return (
                null,
                "A Mechanic profile already exists for this user."
            );
        }

        var mechanic = new Mechanic
        {
            UserId = dto.UserId,
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email?.Trim(),
            Specialization = dto.Specialization?.Trim(),
            ExperienceYears = dto.ExperienceYears,
            IsActive = dto.IsActive
        };

        var createdMechanic =
            await _repository.CreateAsync(mechanic);

        return (MapToResponse(createdMechanic), null);
    }

    public async Task<(MechanicResponseDto? Mechanic, string? Error)> UpdateAsync(
    int id,
    MechanicUpdateDto dto)
    {
        var mechanic =
            await _repository.GetTrackedByIdAsync(id);

        if (mechanic == null)
        {
            return (null, "Mechanic not found.");
        }

        var email = dto.Email?.Trim();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailExists =
                await _repository.UserEmailExistsAsync(
                    email,
                    mechanic.UserId);

            if (emailExists)
            {
                return (
                    null,
                    "This email is already registered to another user."
                );
            }
        }

        mechanic.FullName = dto.FullName.Trim();
        mechanic.Phone = dto.Phone.Trim();
        mechanic.Email = email;
        mechanic.Specialization = dto.Specialization?.Trim();
        mechanic.ExperienceYears = dto.ExperienceYears;
        mechanic.IsActive = dto.IsActive;

        await _repository.UpdateAsync(mechanic);

        var updatedMechanic =
            await _repository.GetByIdAsync(id);

        return updatedMechanic == null
            ? (null, "Mechanic was updated but could not be retrieved.")
            : (MapToResponse(updatedMechanic), null);
    }
    
    public async Task<(bool Deleted, string? Error)> DeleteAsync(int id)
    {
        var mechanic =
            await _repository.GetTrackedByIdAsync(id);

        if (mechanic == null)
        {
            return (false, "Mechanic not found.");
        }

        // Keep service history intact. A mechanic referenced by any service job
        // cannot be deleted until those jobs have been reassigned.
        if (await _repository.HasAssignedServiceJobsAsync(id))
        {
            return (false,
                "Cannot delete this mechanic because service jobs are assigned to them. Reassign the jobs to another mechanic first.");
        }

        try
        {
            await _repository.DeleteAsync(mechanic);
            return (true, null);
        }
        catch (DbUpdateException)
        {
            // The FK constraint is the final safeguard if a service job is
            // assigned between the check above and the delete operation.
            return (false,
                "Cannot delete this mechanic because they are referenced by service jobs. Reassign the jobs first.");
        }
    }

    private static MechanicResponseDto MapToResponse(
        Mechanic mechanic)
    {
        return new MechanicResponseDto
        {
            Id = mechanic.Id,
            UserId = mechanic.UserId,
            FullName = mechanic.FullName,
            Phone = mechanic.Phone,
            Email = mechanic.Email,
            Specialization = mechanic.Specialization,
            ExperienceYears = mechanic.ExperienceYears,
            IsActive = mechanic.IsActive,
            CreatedAt = mechanic.CreatedAt,
            ServiceJobCount = mechanic.ServiceJobs?.Count ?? 0
        };
    }
}