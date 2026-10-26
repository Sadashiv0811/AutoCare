using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class InvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .Include(i => i.ServiceJob)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.ServiceJob)
                .ThenInclude(s => s.ServiceJobParts)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Invoice>> GetByCustomerUserIdAsync(int userId)
    {
        return await _context.Invoices
            .Include(i => i.ServiceJob)
                .ThenInclude(s => s.Vehicle)
                    .ThenInclude(v => v.Customer)
            .Where(i => i.ServiceJob.Vehicle.Customer.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdAndCustomerUserIdAsync(int id, int userId)
    {
        return await _context.Invoices
            .Include(i => i.ServiceJob)
                .ThenInclude(s => s.Vehicle)
                    .ThenInclude(v => v.Customer)
            .Where(i => i.Id == id && i.ServiceJob.Vehicle.Customer.UserId == userId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Invoice?> GetTrackedByIdAsync(int id)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<bool> ServiceJobExistsAsync(int serviceJobId)
    {
        return await _context.ServiceJobs
            .AnyAsync(s => s.Id == serviceJobId);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Invoice invoice)
    {
        _context.Invoices.Remove(invoice);

        await _context.SaveChangesAsync();
    }
    public async Task<ServiceJob?> GetServiceJobWithPartsAsync(int serviceJobId)
    {
        return await _context.ServiceJobs
            .Include(s => s.ServiceJobParts)
            .FirstOrDefaultAsync(s => s.Id == serviceJobId);
    }

    public async Task<bool> InvoiceExistsForServiceJobAsync(int serviceJobId, int? excludingInvoiceId = null)
    {
        return await _context.Invoices
            .AnyAsync(i => i.ServiceJobId == serviceJobId &&
                           (!excludingInvoiceId.HasValue || i.Id != excludingInvoiceId.Value));
    }

}