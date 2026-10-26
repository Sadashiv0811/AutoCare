using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class VehicleRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehicle?> GetTrackedByIdAsync(int id)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Vehicle>> GetByCustomerIdAsync(
        int customerId)
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .Where(v => v.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAndCustomerIdAsync(
        int id,
        int customerId)
    {
        return await _context.Vehicles
            .Include(v => v.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(v =>
                v.Id == id &&
                v.CustomerId == customerId);
    }
    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        return await _context.Customers
            .AnyAsync(c => c.Id == customerId);
    }

    public async Task<bool> HasServiceHistoryAsync(int vehicleId)
    {
        // Keep historical service jobs and maintenance records intact.
        return await _context.ServiceJobs
                .AnyAsync(job => job.VehicleId == vehicleId)
            || await _context.MaintenanceRecords
                .AnyAsync(record => record.VehicleId == vehicleId);
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);

        await _context.SaveChangesAsync();

        return vehicle;
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ServiceJob>> GetServiceJobsAsync(int vehicleId)
    {
        return await _context.ServiceJobs
            .Include(s => s.ServiceType)
            .Where(s => s.VehicleId == vehicleId)
            .OrderByDescending(s => s.ScheduledDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsAsync(int vehicleId)
    {
        return await _context.MaintenanceRecords
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.ServiceDate)
            .AsNoTracking()
            .ToListAsync();
    }

}