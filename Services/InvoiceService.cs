using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Services;

public class InvoiceService
{
    private const decimal MaxMoney = 99_999_999.99m;
    private readonly InvoiceRepository _repository;

    public InvoiceService(InvoiceRepository repository) => _repository = repository;

    public async Task<List<InvoiceResponseDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(MapToResponse).ToList();

    public async Task<List<InvoiceResponseDto>> GetMyInvoicesAsync(int userId) =>
        (await _repository.GetByCustomerUserIdAsync(userId)).Select(MapToResponse).ToList();

    public async Task<InvoiceResponseDto?> GetMyInvoiceAsync(int id, int userId)
    {
        var invoice = await _repository.GetByIdAndCustomerUserIdAsync(id, userId);
        return invoice == null ? null : MapToResponse(invoice);
    }

    public async Task<InvoiceResponseDto?> GetByIdAsync(int id)
    {
        var invoice = await _repository.GetByIdAsync(id);
        return invoice == null ? null : MapToResponse(invoice);
    }

    public async Task<InvoiceCreateResult> CreateAsync(InvoiceCreateDto dto)
    {
        var validation = ValidateMoney(dto.ServiceCharges, dto.TaxAmount);
        if (validation != null)
            return new InvoiceCreateResult { ErrorMessage = validation };
        if (dto.ServiceJobId <= 0)
            return new InvoiceCreateResult { ErrorMessage = "A valid service job is required." };

        var serviceJob = await _repository.GetServiceJobWithPartsAsync(dto.ServiceJobId);
        if (serviceJob == null)
            return new InvoiceCreateResult { ErrorMessage = "Service job not found." };

        if (await _repository.InvoiceExistsForServiceJobAsync(dto.ServiceJobId))
            return new InvoiceCreateResult { ErrorMessage = "An invoice already exists for this service job.", Conflict = true };

        var serviceCharges = Money(dto.ServiceCharges);
        var taxAmount = Money(dto.TaxAmount);
        var partsAmount = Money(serviceJob.ServiceJobParts?.Sum(p => p.TotalPrice) ?? 0m);
        var subtotal = Money(serviceCharges + partsAmount);
        var total = Money(subtotal + taxAmount);
        if (!MoneyWithinDatabaseRange(partsAmount, subtotal, total))
            return new InvoiceCreateResult { ErrorMessage = "The invoice total exceeds the supported monetary range." };

        // PaidAt is server-owned. A client-supplied date is deliberately ignored.
        var invoice = new Invoice
        {
            InvoiceNumber = GenerateInvoiceNumber(),
            ServiceJobId = dto.ServiceJobId,
            ServiceCharges = serviceCharges,
            PartsAmount = partsAmount,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            TotalAmount = total,
            IsPaid = dto.IsPaid,
            PaidAt = dto.IsPaid ? DateTime.UtcNow : null
        };

        try
        {
            var created = await _repository.CreateAsync(invoice);
            var details = await _repository.GetByIdAsync(created.Id);
            if (details == null)
                return new InvoiceCreateResult { ErrorMessage = "Invoice was saved but could not be reloaded." };
            return new InvoiceCreateResult { Invoice = MapToResponse(details) };
        }
        catch (DbUpdateException)
        {
            // Unique indexes on InvoiceNumber and ServiceJobId arbitrate concurrent requests.
            return new InvoiceCreateResult
            {
                ErrorMessage = "An invoice may already exist for this service job, or a unique invoice number conflicted. Refresh and try again.",
                Conflict = true
            };
        }
    }

    public async Task<InvoiceResponseDto?> UpdateAsync(int id, InvoiceUpdateDto dto)
    {
        var invoice = await _repository.GetTrackedByIdAsync(id);
        if (invoice == null) return null;

        var validation = ValidateMoney(dto.ServiceCharges, dto.TaxAmount);
        if (validation != null) throw new ArgumentException(validation);

        // An issued invoice is permanently linked to its original service job and invoice number.
        if (dto.ServiceJobId != invoice.ServiceJobId)
            throw new InvalidOperationException("An invoice cannot be moved to a different service job.");

        var serviceJob = await _repository.GetServiceJobWithPartsAsync(invoice.ServiceJobId);
        if (serviceJob == null)
            throw new InvalidOperationException("The service job linked to this invoice no longer exists.");

        var serviceCharges = Money(dto.ServiceCharges);
        var taxAmount = Money(dto.TaxAmount);
        var partsAmount = Money(serviceJob.ServiceJobParts?.Sum(p => p.TotalPrice) ?? 0m);
        var subtotal = Money(serviceCharges + partsAmount);
        var total = Money(subtotal + taxAmount);
        if (!MoneyWithinDatabaseRange(partsAmount, subtotal, total))
            throw new ArgumentException("The invoice total exceeds the supported monetary range.");

        var wasPaid = invoice.IsPaid;
        invoice.ServiceCharges = serviceCharges;
        invoice.PartsAmount = partsAmount;
        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxAmount;
        invoice.TotalAmount = total;
        invoice.IsPaid = dto.IsPaid;
        // Never trust PaidAt from a request. Preserve the original payment time when still paid;
        // set it on the unpaid -> paid transition, and clear it when marked unpaid.
        invoice.PaidAt = !dto.IsPaid ? null : wasPaid && invoice.PaidAt.HasValue ? invoice.PaidAt : DateTime.UtcNow;

        await _repository.UpdateAsync(invoice);
        var updated = await _repository.GetByIdAsync(id);
        return updated == null ? null : MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _repository.GetTrackedByIdAsync(id);
        if (invoice == null) return false;
        if (invoice.IsPaid)
            throw new InvalidOperationException("A paid invoice cannot be deleted. Use an audited correction or refund process instead.");

        await _repository.DeleteAsync(invoice);
        return true;
    }

    private static string? ValidateMoney(decimal serviceCharges, decimal taxAmount)
    {
        if (serviceCharges < 0) return "Service charges cannot be negative.";
        if (taxAmount < 0) return "Tax amount cannot be negative.";
        if (serviceCharges > MaxMoney || taxAmount > MaxMoney)
            return "Service charges and tax must not exceed 99,999,999.99.";
        return null;
    }

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static bool MoneyWithinDatabaseRange(params decimal[] values) =>
        values.All(v => v >= 0 && v <= MaxMoney);

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..24].ToUpperInvariant();

    private static InvoiceResponseDto MapToResponse(Invoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        ServiceJobId = invoice.ServiceJobId,
        JobNumber = invoice.ServiceJob?.JobNumber ?? string.Empty,
        ServiceCharges = invoice.ServiceCharges,
        PartsAmount = invoice.PartsAmount,
        Subtotal = invoice.Subtotal,
        TaxAmount = invoice.TaxAmount,
        TotalAmount = invoice.TotalAmount,
        IsPaid = invoice.IsPaid,
        PaidAt = invoice.PaidAt,
        CreatedAt = invoice.CreatedAt
    };
}
