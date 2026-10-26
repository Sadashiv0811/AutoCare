using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class DashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Select(i => new InvoiceDashboardData
            {
                TotalAmount = i.TotalAmount,
                IsPaid = i.IsPaid,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        var serviceJobs = await _context.ServiceJobs
            .AsNoTracking()
            .Select(s => new ServiceJobDashboardData
            {
                Status = s.Status,
                ServiceTypeId = s.ServiceTypeId,
                ServiceTypeName = s.ServiceType.Name
            })
            .ToListAsync();

        var vehicles = await _context.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleDashboardData
            {
                Id = v.Id,
                Make = v.Make,
                FuelType = v.FuelType,
                ServiceJobCount = v.ServiceJobs.Count
            })
            .ToListAsync();

        var parts = await _context.Parts
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new PartDashboardData
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                UnitPrice = p.UnitPrice
            })
            .ToListAsync();

        return new DashboardData
        {
            Invoices = invoices,
            ServiceJobs = serviceJobs,
            Vehicles = vehicles,
            Parts = parts,
            MonthStart = monthStart,
            NextMonthStart = nextMonthStart
        };
    }

    public async Task<List<InvoiceDashboardData>> GetInvoicesAsync(
        DateTime fromDate,
        DateTime toDate)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.CreatedAt >= fromDate && i.CreatedAt < toDate)
            .Select(i => new InvoiceDashboardData
            {
                TotalAmount = i.TotalAmount,
                IsPaid = i.IsPaid,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ServiceJobDashboardData>> GetServiceJobsAsync(
        DateTime fromDate,
        DateTime toDate)
    {
        return await _context.ServiceJobs
            .AsNoTracking()
            .Where(s => s.ScheduledDate >= fromDate && s.ScheduledDate < toDate)
            .Select(s => new ServiceJobDashboardData
            {
                Status = s.Status,
                ServiceTypeId = s.ServiceTypeId,
                ServiceTypeName = s.ServiceType.Name
            })
            .ToListAsync();
    }

    public async Task<List<VehicleDashboardData>> GetVehiclesAsync()
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleDashboardData
            {
                Id = v.Id,
                Make = v.Make,
                FuelType = v.FuelType,
                ServiceJobCount = v.ServiceJobs.Count
            })
            .ToListAsync();
    }

    public async Task<List<PartDashboardData>> GetActivePartsAsync()
    {
        return await _context.Parts
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new PartDashboardData
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Quantity = p.Quantity,
                MinimumStock = p.MinimumStock,
                UnitPrice = p.UnitPrice
            })
            .ToListAsync();
    }
}

public class DashboardData
{
    public List<InvoiceDashboardData> Invoices { get; set; } = new();
    public List<ServiceJobDashboardData> ServiceJobs { get; set; } = new();
    public List<VehicleDashboardData> Vehicles { get; set; } = new();
    public List<PartDashboardData> Parts { get; set; } = new();
    public DateTime MonthStart { get; set; }
    public DateTime NextMonthStart { get; set; }
}

public class InvoiceDashboardData
{
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceJobDashboardData
{
    public ServiceJobStatus Status { get; set; }
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
}

public class VehicleDashboardData
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public int ServiceJobCount { get; set; }
}

public class PartDashboardData
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
}
