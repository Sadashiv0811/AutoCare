using System.Data;
using AutoCare.Api.Data;
using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Services;

public class ServiceJobPartService
{
    private readonly ServiceJobPartRepository _repository;
    private readonly AppDbContext _context;

    public ServiceJobPartService(ServiceJobPartRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<List<ServiceJobPartResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(MapToResponse).ToList();

    public async Task<ServiceJobPartResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item == null ? null : MapToResponse(item);
    }

    public async Task<List<PartResponseDto>> GetLowStockPartsAsync()
    {
        var parts = await _repository.GetLowStockPartsAsync();
        return parts.Select(p => new PartResponseDto
        {
            Id = p.Id, PartNumber = p.PartNumber, Name = p.Name,
            Description = p.Description, Quantity = p.Quantity,
            MinimumStock = p.MinimumStock, UnitPrice = p.UnitPrice,
            Supplier = p.Supplier, IsActive = p.IsActive,
            CreatedAt = p.CreatedAt, IsLowStock = p.Quantity <= p.MinimumStock
        }).ToList();
    }

    public async Task<ServiceJobPartResponseDto?> CreateAsync(ServiceJobPartCreateDto dto)
    {
        if (dto.Quantity <= 0) return null;
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (!await _repository.ServiceJobExistsAsync(dto.ServiceJobId)) return null;
        if (await _repository.JobHasInvoiceAsync(dto.ServiceJobId))
            throw new InvalidOperationException("Parts cannot be added or changed after an invoice has been created for this service job.");

        var part = await _repository.GetPartByIdAsync(dto.PartId);
        if (part == null || !part.IsActive || part.Quantity < dto.Quantity) return null;

        var existing = await _repository.GetTrackedByServiceJobAndPartAsync(dto.ServiceJobId, dto.PartId);
        part.Quantity -= dto.Quantity;

        if (existing != null)
        {
            existing.Quantity = checked(existing.Quantity + dto.Quantity);
            existing.TotalPrice = existing.UnitPrice * existing.Quantity;
            await _repository.UpdateAsync(existing);
            await transaction.CommitAsync();
            var updated = await _repository.GetByIdAsync(existing.Id);
            return updated == null ? null : MapToResponse(updated);
        }

        var assignment = new ServiceJobPart
        {
            ServiceJobId = dto.ServiceJobId,
            PartId = dto.PartId,
            Quantity = dto.Quantity,
            UnitPrice = part.UnitPrice,
            TotalPrice = part.UnitPrice * dto.Quantity
        };
        var created = await _repository.CreateAsync(assignment);
        await transaction.CommitAsync();
        var details = await _repository.GetByIdAsync(created.Id);
        return details == null ? null : MapToResponse(details);
    }

    public async Task<ServiceJobPartResponseDto?> UpdateAsync(int id, ServiceJobPartUpdateDto dto)
    {
        if (dto.Quantity <= 0) return null;
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var existing = await _repository.GetTrackedByIdAsync(id);
        if (existing == null || !await _repository.ServiceJobExistsAsync(dto.ServiceJobId)) return null;

        if (await _repository.JobHasInvoiceAsync(existing.ServiceJobId) ||
            await _repository.JobHasInvoiceAsync(dto.ServiceJobId))
            throw new InvalidOperationException("Parts cannot be edited or moved after an invoice has been created for the source or target service job.");

        var newPart = await _repository.GetPartByIdAsync(dto.PartId);
        if (newPart == null || !newPart.IsActive) return null;

        var duplicate = await _repository.GetTrackedByServiceJobAndPartAsync(dto.ServiceJobId, dto.PartId);
        if (duplicate != null && duplicate.Id != id) return null;

        var oldPart = existing.Part;
        var oldQuantity = existing.Quantity;
        var samePart = existing.PartId == dto.PartId;
        var available = newPart.Quantity + (samePart ? oldQuantity : 0);
        if (available < dto.Quantity) return null;

        if (samePart)
        {
            newPart.Quantity = available - dto.Quantity;
        }
        else
        {
            oldPart.Quantity = checked(oldPart.Quantity + oldQuantity);
            newPart.Quantity -= dto.Quantity;
        }

        existing.ServiceJobId = dto.ServiceJobId;
        existing.PartId = dto.PartId;
        existing.Quantity = dto.Quantity;
        // Keep the original price snapshot if the part is unchanged. When the
        // part changes, snapshot the replacement part's current unit price.
        if (!samePart) existing.UnitPrice = newPart.UnitPrice;
        existing.TotalPrice = existing.UnitPrice * dto.Quantity;

        await _repository.UpdateAsync(existing);
        await transaction.CommitAsync();
        var updated = await _repository.GetByIdAsync(id);
        return updated == null ? null : MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existing = await _repository.GetTrackedByIdAsync(id);
        if (existing == null) return false;

        if (await _repository.JobHasInvoiceAsync(existing.ServiceJobId))
            throw new InvalidOperationException("A part assignment cannot be deleted after an invoice has been created for its service job.");

        existing.Part.Quantity = checked(existing.Part.Quantity + existing.Quantity);
        await _repository.DeleteAsync(existing);
        await transaction.CommitAsync();
        return true;
    }

    private static ServiceJobPartResponseDto MapToResponse(ServiceJobPart item) => new()
    {
        Id = item.Id,
        ServiceJobId = item.ServiceJobId,
        JobNumber = item.ServiceJob?.JobNumber ?? string.Empty,
        PartId = item.PartId,
        PartNumber = item.Part?.PartNumber ?? string.Empty,
        PartName = item.Part?.Name ?? string.Empty,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        TotalPrice = item.TotalPrice
    };
}
