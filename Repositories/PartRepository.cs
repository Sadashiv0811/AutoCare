using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class PartRepository
{
    private readonly AppDbContext _context;

    public PartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Part>> GetAllAsync()
    {
        return await _context.Parts
            .Include(p => p.ServiceJobParts)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Part?> GetByIdAsync(int id)
    {
        return await _context.Parts
            .Include(p => p.ServiceJobParts)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Part?> GetTrackedByIdAsync(int id)
    {
        return await _context.Parts
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> PartNumberExistsAsync(string partNumber, int? exceptId = null)
    {
        var normalizedNumber = partNumber.Trim().ToLower();
        return await _context.Parts.AnyAsync(p =>
            p.PartNumber.ToLower() == normalizedNumber &&
            (!exceptId.HasValue || p.Id != exceptId.Value));
    }

    public async Task<bool> HasServiceJobAssignmentsAsync(int id)
    {
        return await _context.ServiceJobParts.AnyAsync(sp => sp.PartId == id);
    }

    public async Task<Part> CreateAsync(Part part)
    {
        _context.Parts.Add(part);

        await _context.SaveChangesAsync();

        return part;
    }

    public async Task UpdateAsync(Part part)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Part part)
    {
        _context.Parts.Remove(part);

        await _context.SaveChangesAsync();
    }

    public async Task<List<Part>> GetLowStockAsync()
    {
        return await _context.Parts
            .Where(p => p.IsActive && p.Quantity <= p.MinimumStock)
            .AsNoTracking()
            .ToListAsync();
    }

}