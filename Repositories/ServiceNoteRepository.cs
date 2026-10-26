using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class ServiceNoteRepository
{
    private readonly AppDbContext _context;

    public ServiceNoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceNote>> GetAllAsync()
    {
        return await _context.ServiceNotes
            .Include(n => n.ServiceJob)
            .Include(n => n.Mechanic)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceNote?> GetByIdAsync(int id)
    {
        return await _context.ServiceNotes
            .Include(n => n.ServiceJob)
            .Include(n => n.Mechanic)
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<ServiceNote?> GetTrackedByIdAsync(int id)
    {
        return await _context.ServiceNotes
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<bool> ServiceJobExistsAsync(int serviceJobId)
    {
        return await _context.ServiceJobs
            .AnyAsync(s => s.Id == serviceJobId);
    }

    public async Task<bool> MechanicExistsAsync(int mechanicId)
    {
        return await _context.Mechanics
            .AnyAsync(m => m.Id == mechanicId);
    }

    public async Task<ServiceNote> CreateAsync(
        ServiceNote serviceNote)
    {
        _context.ServiceNotes.Add(serviceNote);

        await _context.SaveChangesAsync();

        return serviceNote;
    }

    public async Task UpdateAsync(
        ServiceNote serviceNote)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        ServiceNote serviceNote)
    {
        _context.ServiceNotes.Remove(serviceNote);

        await _context.SaveChangesAsync();
    }
}