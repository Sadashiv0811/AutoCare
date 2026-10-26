using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class ServiceJobRepository
{
    private readonly AppDbContext _context;

    public ServiceJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceJob>> GetAllAsync()
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceJob?> GetByIdAsync(int id)
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<ServiceJob>> GetByCustomerUserIdAsync(int userId)
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .Where(s => s.Vehicle.Customer.UserId == userId)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceJob?> GetByIdAndCustomerUserIdAsync(int id, int userId)
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .Where(s => s.Id == id && s.Vehicle.Customer.UserId == userId)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<ServiceJob>> GetByMechanicUserIdAsync(int userId)
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .Where(s => s.Mechanic.UserId == userId)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceJob?> GetByIdAndMechanicUserIdAsync(int id, int userId)
    {
        return await _context.ServiceJobs
            .Include(s => s.Vehicle)
            .Include(s => s.Mechanic)
            .Include(s => s.ServiceType)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .Include(s => s.Invoice)
            .Where(s => s.Id == id && s.Mechanic.UserId == userId)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceJob?> GetTrackedByIdAsync(int id)
    {
        return await _context.ServiceJobs
            .Include(s => s.ServiceJobParts)
                .ThenInclude(sp => sp.Part)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> VehicleExistsAsync(int vehicleId)
    {
        return await _context.Vehicles
            .AnyAsync(v => v.Id == vehicleId);
    }

    public async Task<bool> MechanicExistsAsync(int mechanicId)
    {
        return await _context.Mechanics
            .AnyAsync(m => m.Id == mechanicId);
    }

    public async Task<bool> MechanicIsActiveAsync(int mechanicId)
    {
        return await _context.Mechanics
            .AnyAsync(m => m.Id == mechanicId && m.IsActive);
    }

    public async Task<bool> ServiceTypeExistsAsync(int serviceTypeId)
    {
        return await _context.ServiceTypes
            .AnyAsync(s => s.Id == serviceTypeId);
    }

    public async Task<bool> ServiceTypeIsActiveAsync(int serviceTypeId)
    {
        return await _context.ServiceTypes
            .AnyAsync(s => s.Id == serviceTypeId && s.IsActive);
    }

    public async Task<bool> HasInvoiceAsync(int serviceJobId)
    {
        return await _context.Invoices
            .AnyAsync(i => i.ServiceJobId == serviceJobId);
    }

    public async Task<ServiceJob> CreateAsync(ServiceJob serviceJob)
    {
        _context.ServiceJobs.Add(serviceJob);

        await _context.SaveChangesAsync();

        return serviceJob;
    }

    public async Task UpdateAsync(ServiceJob serviceJob)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServiceJob serviceJob)
    {
        _context.ServiceJobs.Remove(serviceJob);

        await _context.SaveChangesAsync();
    }

    public async Task<ServiceJob?> GetTrackedByIdAndMechanicUserIdAsync(
        int id,
        int userId)
    {
        return await _context.ServiceJobs
            .Include(s => s.Mechanic)
            .Include(s => s.Vehicle)
            .Include(s => s.ServiceType)
            .Include(s => s.Invoice)
            .Include(s => s.ServiceJobParts)
            .Include(s => s.ServiceNotes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.Mechanic.UserId == userId);
    }

}

// A Service Job has relationships with:
// ServiceJob
//    │
//    ├── Vehicle
//    ├── Mechanic
//    ├── ServiceType
//    ├── ServiceJobParts
//    ├── ServiceNotes
//    └── Invoice

// What AsSplitQuery() does
// Instead of one giant query:
// ServiceJob
//    + Parts
//    + Notes
//    + Vehicle
//    + Mechanic
//    + ServiceType
//    + Invoice

// EF Core splits the operation into multiple SQL queries and combines the results.
// Conceptually:
// Query 1 → ServiceJobs + single relationships
// Query 2 → ServiceJobParts
// Query 3 → ServiceNotes
// This avoids the multiplication problem.