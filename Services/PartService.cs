using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class PartService
{
    private readonly PartRepository _repository;

    public PartService(PartRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PartResponseDto>> GetAllAsync()
    {
        var parts = await _repository.GetAllAsync();

        return parts
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<List<PartResponseDto>> GetLowStockAsync()
    {
        var parts = await _repository.GetLowStockAsync();
        return parts.Select(MapToResponse).ToList();
    }

    public async Task<PartResponseDto?> GetByIdAsync(int id)
    {
        var part = await _repository.GetByIdAsync(id);

        return part == null
            ? null
            : MapToResponse(part);
    }

    public async Task<PartResponseDto?> CreateAsync(
        PartCreateDto dto)
    {
        if (!IsValid(dto.PartNumber, dto.Name, dto.Description, dto.Quantity,
                dto.MinimumStock, dto.UnitPrice, dto.Supplier) ||
            await _repository.PartNumberExistsAsync(dto.PartNumber))
        {
            return null;
        }

        var part = new Part
        {
            PartNumber = dto.PartNumber.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Quantity = dto.Quantity,
            MinimumStock = dto.MinimumStock,
            UnitPrice = dto.UnitPrice,
            Supplier = dto.Supplier?.Trim(),
            IsActive = dto.IsActive
        };

        var createdPart =
            await _repository.CreateAsync(part);

        return MapToResponse(createdPart);
    }

    public async Task<PartResponseDto?> UpdateAsync(
        int id,
        PartUpdateDto dto)
    {
        var part =
            await _repository.GetTrackedByIdAsync(id);

        if (part == null)
        {
            return null;
        }

        if (!IsValid(dto.PartNumber, dto.Name, dto.Description, dto.Quantity,
                dto.MinimumStock, dto.UnitPrice, dto.Supplier) ||
            await _repository.PartNumberExistsAsync(dto.PartNumber, id))
        {
            return null;
        }

        part.PartNumber = dto.PartNumber.Trim();
        part.Name = dto.Name.Trim();
        part.Description = dto.Description?.Trim();
        part.Quantity = dto.Quantity;
        part.MinimumStock = dto.MinimumStock;
        part.UnitPrice = dto.UnitPrice;
        part.Supplier = dto.Supplier?.Trim();
        part.IsActive = dto.IsActive;

        await _repository.UpdateAsync(part);

        var updatedPart =
            await _repository.GetByIdAsync(id);

        return updatedPart == null
            ? null
            : MapToResponse(updatedPart);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var part =
            await _repository.GetTrackedByIdAsync(id);

        if (part == null || await _repository.HasServiceJobAssignmentsAsync(id))
        {
            return false;
        }

        await _repository.DeleteAsync(part);

        return true;
    }

    private static bool IsValid(
        string? partNumber,
        string? name,
        string? description,
        int quantity,
        int minimumStock,
        decimal unitPrice,
        string? supplier)
    {
        return !string.IsNullOrWhiteSpace(partNumber) &&
               partNumber.Trim().Length <= 50 &&
               !string.IsNullOrWhiteSpace(name) &&
               name.Trim().Length <= 100 &&
               (description == null || description.Trim().Length <= 500) &&
               quantity >= 0 &&
               minimumStock >= 0 &&
               unitPrice >= 0 &&
               unitPrice <= 9999999999.99m &&
               (supplier == null || supplier.Trim().Length <= 150);
    }

    private static PartResponseDto MapToResponse(
        Part part)
    {
        return new PartResponseDto
        {
            Id = part.Id,
            PartNumber = part.PartNumber,
            Name = part.Name,
            Description = part.Description,
            Quantity = part.Quantity,
            MinimumStock = part.MinimumStock,
            UnitPrice = part.UnitPrice,
            Supplier = part.Supplier,
            IsActive = part.IsActive,
            CreatedAt = part.CreatedAt,
            ServiceJobPartCount =
                part.ServiceJobParts?.Count ?? 0,
            IsLowStock =
                part.Quantity <= part.MinimumStock
        };
    }
}