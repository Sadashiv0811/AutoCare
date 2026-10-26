using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class MaintenanceRecordRepository
{
    private readonly AppDbContext _context;

    public MaintenanceRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceRecord>> GetAllAsync()
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Include(m => m.ServiceJob)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MaintenanceRecord?> GetByIdAsync(int id)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
            .Include(m => m.ServiceJob)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<MaintenanceRecord>> GetByCustomerUserIdAsync(int userId)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(m => m.ServiceJob)
            .Where(m => m.Vehicle.Customer.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MaintenanceRecord?> GetByIdAndCustomerUserIdAsync(int id, int userId)
    {
        return await _context.MaintenanceRecords
            .Include(m => m.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(m => m.ServiceJob)
            .Where(m => m.Id == id && m.Vehicle.Customer.UserId == userId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<MaintenanceRecord?> GetTrackedByIdAsync(int id)
    {
        return await _context.MaintenanceRecords
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> VehicleExistsAsync(int vehicleId)
    {
        return await _context.Vehicles
            .AnyAsync(v => v.Id == vehicleId);
    }

    public async Task<bool> ServiceJobExistsAsync(int serviceJobId)
    {
        return await _context.ServiceJobs
            .AnyAsync(s => s.Id == serviceJobId);
    }

    public async Task<MaintenanceRecord> CreateAsync(
        MaintenanceRecord record)
    {
        _context.MaintenanceRecords.Add(record);

        await _context.SaveChangesAsync();

        return record;
    }

    public async Task UpdateAsync(
        MaintenanceRecord record)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        MaintenanceRecord record)
    {
        _context.MaintenanceRecords.Remove(record);

        await _context.SaveChangesAsync();
    }
}