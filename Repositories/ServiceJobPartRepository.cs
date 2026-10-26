using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class ServiceJobPartRepository
{
    private readonly AppDbContext _context;

    public ServiceJobPartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceJobPart>> GetAllAsync()
    {
        return await _context.ServiceJobParts
            .Include(s => s.ServiceJob)
            .Include(s => s.Part)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceJobPart?> GetByIdAsync(int id)
    {
        return await _context.ServiceJobParts
            .Include(s => s.ServiceJob)
            .Include(s => s.Part)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServiceJobPart?> GetTrackedByIdAsync(int id)
    {
        return await _context.ServiceJobParts
            .Include(s => s.Part)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServiceJobPart?> GetTrackedByServiceJobAndPartAsync(
    int serviceJobId,
    int partId)
    {
        return await _context.ServiceJobParts
            .Include(x => x.Part)
            .FirstOrDefaultAsync(x =>
                x.ServiceJobId == serviceJobId &&
                x.PartId == partId);
    }

    public async Task<bool> ServiceJobExistsAsync(int serviceJobId)
    {
        return await _context.ServiceJobs.AnyAsync(s => s.Id == serviceJobId);
    }

    public async Task<bool> JobHasInvoiceAsync(int serviceJobId)
    {
        return await _context.Invoices.AnyAsync(i => i.ServiceJobId == serviceJobId);
    }

    public async Task<Part?> GetPartByIdAsync(int partId)
    {
        return await _context.Parts.FirstOrDefaultAsync(p => p.Id == partId);
    }

    public async Task<List<Part>> GetLowStockPartsAsync()
    {
        return await _context.Parts
            .Where(p => p.IsActive && p.Quantity <= p.MinimumStock)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceJobPart> CreateAsync(ServiceJobPart serviceJobPart)
    {
        _context.ServiceJobParts.Add(serviceJobPart);
        await _context.SaveChangesAsync();
        return serviceJobPart;
    }

    public async Task UpdateAsync(ServiceJobPart serviceJobPart)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServiceJobPart serviceJobPart)
    {
        _context.ServiceJobParts.Remove(serviceJobPart);
        await _context.SaveChangesAsync();
    }
}
