using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class ServiceTypeRepository
{
    private readonly AppDbContext _context;

    public ServiceTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceType>> GetAllAsync()
    {
        return await _context.ServiceTypes
            .Include(s => s.ServiceJobs)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceType?> GetByIdAsync(int id)
    {
        return await _context.ServiceTypes
            .Include(s => s.ServiceJobs)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServiceType?> GetTrackedByIdAsync(int id)
    {
        return await _context.ServiceTypes
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name, int? exceptId = null)
    {
        var normalizedName = name.Trim().ToLower();
        return await _context.ServiceTypes.AnyAsync(s =>
            s.Name.ToLower() == normalizedName &&
            (!exceptId.HasValue || s.Id != exceptId.Value));
    }

    public async Task<bool> HasServiceJobsAsync(int id)
    {
        return await _context.ServiceJobs.AnyAsync(s => s.ServiceTypeId == id);
    }

    public async Task<ServiceType> CreateAsync(ServiceType serviceType)
    {
        _context.ServiceTypes.Add(serviceType);

        await _context.SaveChangesAsync();

        return serviceType;
    }

    public async Task UpdateAsync(ServiceType serviceType)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServiceType serviceType)
    {
        _context.ServiceTypes.Remove(serviceType);

        await _context.SaveChangesAsync();
    }
}