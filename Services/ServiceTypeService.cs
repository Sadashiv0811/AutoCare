using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class ServiceTypeService
{
    private readonly ServiceTypeRepository _repository;

    public ServiceTypeService(ServiceTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ServiceTypeResponseDto>> GetAllAsync()
    {
        var serviceTypes = await _repository.GetAllAsync();

        return serviceTypes
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ServiceTypeResponseDto?> GetByIdAsync(int id)
    {
        var serviceType = await _repository.GetByIdAsync(id);

        return serviceType == null
            ? null
            : MapToResponse(serviceType);
    }

    public async Task<ServiceTypeResponseDto?> CreateAsync(
        ServiceTypeCreateDto dto)
    {
        if (!IsValid(dto.Name, dto.Description, dto.EstimatedDurationMinutes, dto.BasePrice) ||
            await _repository.NameExistsAsync(dto.Name))
        {
            return null;
        }

        var serviceType = new ServiceType
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
            BasePrice = dto.BasePrice,
            IsActive = dto.IsActive
        };

        var createdServiceType =
            await _repository.CreateAsync(serviceType);

        return MapToResponse(createdServiceType);
    }

    public async Task<ServiceTypeResponseDto?> UpdateAsync(
        int id,
        ServiceTypeUpdateDto dto)
    {
        var serviceType =
            await _repository.GetTrackedByIdAsync(id);

        if (serviceType == null)
        {
            return null;
        }

        if (!IsValid(dto.Name, dto.Description, dto.EstimatedDurationMinutes, dto.BasePrice) ||
            await _repository.NameExistsAsync(dto.Name, id))
        {
            return null;
        }

        serviceType.Name = dto.Name.Trim();
        serviceType.Description = dto.Description?.Trim();
        serviceType.EstimatedDurationMinutes =
            dto.EstimatedDurationMinutes;
        serviceType.BasePrice = dto.BasePrice;
        serviceType.IsActive = dto.IsActive;

        await _repository.UpdateAsync(serviceType);

        var updatedServiceType =
            await _repository.GetByIdAsync(id);

        return updatedServiceType == null
            ? null
            : MapToResponse(updatedServiceType);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var serviceType =
            await _repository.GetTrackedByIdAsync(id);

        if (serviceType == null || await _repository.HasServiceJobsAsync(id))
        {
            return false;
        }

        await _repository.DeleteAsync(serviceType);

        return true;
    }

    private static bool IsValid(
        string? name,
        string? description,
        int duration,
        decimal price)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.Trim().Length <= 100 &&
               (description == null || description.Length <= 500) &&
               duration > 0 &&
               price >= 0;
    }

    private static ServiceTypeResponseDto MapToResponse(
        ServiceType serviceType)
    {
        return new ServiceTypeResponseDto
        {
            Id = serviceType.Id,
            Name = serviceType.Name,
            Description = serviceType.Description,
            EstimatedDurationMinutes =
                serviceType.EstimatedDurationMinutes,
            BasePrice = serviceType.BasePrice,
            IsActive = serviceType.IsActive,
            ServiceJobCount =
                serviceType.ServiceJobs?.Count ?? 0
        };
    }
}