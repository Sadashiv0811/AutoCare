using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class ServiceNoteService
{
    private readonly ServiceNoteRepository _repository;

    public ServiceNoteService(
        ServiceNoteRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ServiceNoteResponseDto>> GetAllAsync()
    {
        var notes = await _repository.GetAllAsync();

        return notes
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ServiceNoteResponseDto?> GetByIdAsync(
        int id)
    {
        var note = await _repository.GetByIdAsync(id);

        return note == null
            ? null
            : MapToResponse(note);
    }

    public async Task<ServiceNoteResponseDto?> CreateAsync(
        ServiceNoteCreateDto dto)
    {
        var serviceJobExists =
            await _repository.ServiceJobExistsAsync(
                dto.ServiceJobId);

        if (!serviceJobExists)
        {
            return null;
        }

        if (dto.MechanicId.HasValue)
        {
            var mechanicExists =
                await _repository.MechanicExistsAsync(
                    dto.MechanicId.Value);

            if (!mechanicExists)
            {
                return null;
            }
        }

        var serviceNote = new ServiceNote
        {
            ServiceJobId = dto.ServiceJobId,

            MechanicId = dto.MechanicId,

            Note = dto.Note.Trim()
        };

        var createdNote =
            await _repository.CreateAsync(serviceNote);

        var noteWithDetails =
            await _repository.GetByIdAsync(
                createdNote.Id);

        return noteWithDetails == null
            ? null
            : MapToResponse(noteWithDetails);
    }

    public async Task<ServiceNoteResponseDto?> UpdateAsync(
        int id,
        ServiceNoteUpdateDto dto)
    {
        var serviceNote =
            await _repository.GetTrackedByIdAsync(id);

        if (serviceNote == null)
        {
            return null;
        }

        var serviceJobExists =
            await _repository.ServiceJobExistsAsync(
                dto.ServiceJobId);

        if (!serviceJobExists)
        {
            return null;
        }

        if (dto.MechanicId.HasValue)
        {
            var mechanicExists =
                await _repository.MechanicExistsAsync(
                    dto.MechanicId.Value);

            if (!mechanicExists)
            {
                return null;
            }
        }

        serviceNote.ServiceJobId =
            dto.ServiceJobId;

        serviceNote.MechanicId =
            dto.MechanicId;

        serviceNote.Note =
            dto.Note.Trim();

        await _repository.UpdateAsync(serviceNote);

        var updatedNote =
            await _repository.GetByIdAsync(id);

        return updatedNote == null
            ? null
            : MapToResponse(updatedNote);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var serviceNote =
            await _repository.GetTrackedByIdAsync(id);

        if (serviceNote == null)
        {
            return false;
        }

        await _repository.DeleteAsync(serviceNote);

        return true;
    }

    private static ServiceNoteResponseDto MapToResponse(
        ServiceNote serviceNote)
    {
        return new ServiceNoteResponseDto
        {
            Id = serviceNote.Id,

            ServiceJobId =
                serviceNote.ServiceJobId,

            JobNumber =
                serviceNote.ServiceJob?.JobNumber
                ?? string.Empty,

            MechanicId =
                serviceNote.MechanicId,

            MechanicName =
                serviceNote.Mechanic?.FullName,

            Note =
                serviceNote.Note,

            CreatedAt =
                serviceNote.CreatedAt
        };
    }
}