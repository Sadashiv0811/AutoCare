using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using System.Globalization;

namespace AutoCare.Api.Services;

public class DashboardService
{
    private readonly DashboardRepository _repository;

    public DashboardService(DashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var data = await _repository.GetDashboardDataAsync();

        return new DashboardSummaryDto
        {
            Revenue = BuildRevenueSummary(data),
            Services = BuildServiceSummary(data.ServiceJobs),
            Vehicles = BuildVehicleSummary(data.Vehicles),
            Inventory = BuildInventorySummary(data.Parts)
        };
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var endExclusive = (toDate ?? DateTime.UtcNow).Date.AddDays(1);
        var start = (fromDate ?? endExclusive.AddMonths(-12)).Date;

        if (start >= endExclusive)
        {
            throw new ArgumentException("fromDate must be earlier than toDate.");
        }

        var invoices = await _repository.GetInvoicesAsync(start, endExclusive);

        var monthly = new List<RevenueByMonthDto>();
        var monthCursor = new DateTime(start.Year, start.Month, 1);

        while (monthCursor < endExclusive)
        {
            var nextMonth = monthCursor.AddMonths(1);

            var monthInvoices = invoices
                .Where(i => i.CreatedAt >= monthCursor && i.CreatedAt < nextMonth)
                .ToList();

            monthly.Add(new RevenueByMonthDto
            {
                Year = monthCursor.Year,
                Month = monthCursor.Month,
                MonthName = monthCursor.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                InvoicedRevenue = monthInvoices.Sum(i => i.TotalAmount),
                PaidRevenue = monthInvoices.Where(i => i.IsPaid).Sum(i => i.TotalAmount),
                InvoiceCount = monthInvoices.Count,
                PaidInvoiceCount = monthInvoices.Count(i => i.IsPaid),
                UnpaidInvoiceCount = monthInvoices.Count(i => !i.IsPaid)
            });

            monthCursor = nextMonth;
        }

        return new RevenueReportDto
        {
            FromDate = start,
            ToDate = endExclusive.AddTicks(-1),
            TotalInvoicedRevenue = invoices.Sum(i => i.TotalAmount),
            TotalPaidRevenue = invoices.Where(i => i.IsPaid).Sum(i => i.TotalAmount),
            TotalUnpaidRevenue = invoices.Where(i => !i.IsPaid).Sum(i => i.TotalAmount),
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.IsPaid),
            UnpaidInvoices = invoices.Count(i => !i.IsPaid),
            MonthlyRevenue = monthly
        };
    }

    public async Task<ServiceReportDto> GetServiceReportAsync(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var endExclusive = (toDate ?? DateTime.UtcNow).Date.AddDays(1);
        var start = (fromDate ?? endExclusive.AddMonths(-12)).Date;

        if (start >= endExclusive)
        {
            throw new ArgumentException("fromDate must be earlier than toDate.");
        }

        var jobs = await _repository.GetServiceJobsAsync(start, endExclusive);

        return new ServiceReportDto
        {
            TotalServiceJobs = jobs.Count,
            ByStatus = Enum.GetValues<ServiceJobStatus>()
                .Select(status => new ServiceStatusReportDto
                {
                    Status = status.ToString(),
                    Count = jobs.Count(j => j.Status == status)
                })
                .ToList(),
            ByServiceType = jobs
                .GroupBy(j => new { j.ServiceTypeId, j.ServiceTypeName })
                .Select(group => new ServiceTypeReportDto
                {
                    ServiceTypeId = group.Key.ServiceTypeId,
                    ServiceTypeName = group.Key.ServiceTypeName,
                    ServiceJobCount = group.Count()
                })
                .OrderByDescending(x => x.ServiceJobCount)
                .ThenBy(x => x.ServiceTypeName)
                .ToList()
        };
    }

    public async Task<VehicleReportDto> GetVehicleReportAsync()
    {
        var vehicles = await _repository.GetVehiclesAsync();

        return new VehicleReportDto
        {
            TotalVehicles = vehicles.Count,
            ServicedVehicles = vehicles.Count(v => v.ServiceJobCount > 0),
            NeverServicedVehicles = vehicles.Count(v => v.ServiceJobCount == 0),
            ByMake = vehicles
                .GroupBy(v => string.IsNullOrWhiteSpace(v.Make) ? "Unknown" : v.Make.Trim())
                .Select(group => new VehicleMakeReportDto
                {
                    Make = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Make)
                .ToList(),
            ByFuelType = vehicles
                .GroupBy(v => string.IsNullOrWhiteSpace(v.FuelType) ? "Unknown" : v.FuelType.Trim())
                .Select(group => new VehicleFuelTypeReportDto
                {
                    FuelType = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.FuelType)
                .ToList()
        };
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync()
    {
        var parts = await _repository.GetActivePartsAsync();

        return new InventoryReportDto
        {
            TotalActiveParts = parts.Count,
            TotalQuantity = parts.Sum(p => p.Quantity),

            LowStockParts = parts.Count(p => p.Quantity <= p.MinimumStock && p.Quantity > 0),

            OutOfStockParts = parts.Count(p => p.Quantity == 0),
            
            InventoryValue = parts.Sum(p => p.Quantity * p.UnitPrice),
            LowStockItems = parts
                .Where(p => p.Quantity <= p.MinimumStock)
                .OrderBy(p => p.Quantity)
                .ThenBy(p => p.Name)
                .Select(p => new LowStockPartDto
                {
                    PartId = p.Id,
                    PartNumber = p.PartNumber,
                    Name = p.Name,
                    Quantity = p.Quantity,
                    MinimumStock = p.MinimumStock,
                    UnitPrice = p.UnitPrice,
                    IsOutOfStock = p.Quantity == 0
                })
                .ToList()
        };
    }

    private static RevenueSummaryDto BuildRevenueSummary(DashboardData data)
    {
        var invoices = data.Invoices;

        return new RevenueSummaryDto
        {
            TotalInvoicedRevenue = invoices.Sum(i => i.TotalAmount),
            TotalPaidRevenue = invoices.Where(i => i.IsPaid).Sum(i => i.TotalAmount),
            TotalUnpaidRevenue = invoices.Where(i => !i.IsPaid).Sum(i => i.TotalAmount),
            ThisMonthInvoicedRevenue = invoices
                .Where(i => i.CreatedAt >= data.MonthStart &&
                            i.CreatedAt < data.NextMonthStart)
                .Sum(i => i.TotalAmount),
            ThisMonthPaidRevenue = invoices
                .Where(i => i.IsPaid &&
                            i.CreatedAt >= data.MonthStart &&
                            i.CreatedAt < data.NextMonthStart)
                .Sum(i => i.TotalAmount),
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.IsPaid),
            UnpaidInvoices = invoices.Count(i => !i.IsPaid)
        };
    }

    private static ServiceSummaryDto BuildServiceSummary(
        List<ServiceJobDashboardData> jobs)
    {
        return new ServiceSummaryDto
        {
            TotalServiceJobs = jobs.Count,
            Scheduled = jobs.Count(j => j.Status == ServiceJobStatus.Scheduled),
            Received = jobs.Count(j => j.Status == ServiceJobStatus.Received),
            Inspection = jobs.Count(j => j.Status == ServiceJobStatus.Inspection),
            InProgress = jobs.Count(j => j.Status == ServiceJobStatus.InProgress),
            Completed = jobs.Count(j => j.Status == ServiceJobStatus.Completed),
            ReadyForPickup = jobs.Count(j => j.Status == ServiceJobStatus.ReadyForPickup),
            Delivered = jobs.Count(j => j.Status == ServiceJobStatus.Delivered)
        };
    }

    private static VehicleSummaryDto BuildVehicleSummary(
        List<VehicleDashboardData> vehicles)
    {
        return new VehicleSummaryDto
        {
            TotalVehicles = vehicles.Count,
            ServicedVehicles = vehicles.Count(v => v.ServiceJobCount > 0),
            NeverServicedVehicles = vehicles.Count(v => v.ServiceJobCount == 0)
        };
    }

    private static InventorySummaryDto BuildInventorySummary(
        List<PartDashboardData> parts)
    {
        return new InventorySummaryDto
        {
            TotalActiveParts = parts.Count,
            TotalQuantity = parts.Sum(p => p.Quantity),
            LowStockParts = parts.Count(p => p.Quantity <= p.MinimumStock),
            OutOfStockParts = parts.Count(p => p.Quantity == 0),
            InventoryValue = parts.Sum(p => p.Quantity * p.UnitPrice)
        };
    }
}
