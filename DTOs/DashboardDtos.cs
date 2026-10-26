namespace AutoCare.Api.DTOs;

public class DashboardSummaryDto
{
    public RevenueSummaryDto Revenue { get; set; } = new();
    public ServiceSummaryDto Services { get; set; } = new();
    public VehicleSummaryDto Vehicles { get; set; } = new();
    public InventorySummaryDto Inventory { get; set; } = new();
}

public class RevenueSummaryDto
{
    public decimal TotalInvoicedRevenue { get; set; }
    public decimal TotalPaidRevenue { get; set; }
    public decimal TotalUnpaidRevenue { get; set; }
    public decimal ThisMonthInvoicedRevenue { get; set; }
    public decimal ThisMonthPaidRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
}

public class ServiceSummaryDto
{
    public int TotalServiceJobs { get; set; }
    public int Scheduled { get; set; }
    public int Received { get; set; }
    public int Inspection { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int ReadyForPickup { get; set; }
    public int Delivered { get; set; }
}

public class VehicleSummaryDto
{
    public int TotalVehicles { get; set; }
    public int ServicedVehicles { get; set; }
    public int NeverServicedVehicles { get; set; }
}

public class InventorySummaryDto
{
    public int TotalActiveParts { get; set; }
    public int TotalQuantity { get; set; }
    public int LowStockParts { get; set; }
    public int OutOfStockParts { get; set; }
    public decimal InventoryValue { get; set; }
}

public class RevenueReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalInvoicedRevenue { get; set; }
    public decimal TotalPaidRevenue { get; set; }
    public decimal TotalUnpaidRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public List<RevenueByMonthDto> MonthlyRevenue { get; set; } = new();
}

public class RevenueByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal InvoicedRevenue { get; set; }
    public decimal PaidRevenue { get; set; }
    public int InvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }
    public int UnpaidInvoiceCount { get; set; }
}

public class ServiceReportDto
{
    public int TotalServiceJobs { get; set; }
    public List<ServiceStatusReportDto> ByStatus { get; set; } = new();
    public List<ServiceTypeReportDto> ByServiceType { get; set; } = new();
}

public class ServiceStatusReportDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ServiceTypeReportDto
{
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public int ServiceJobCount { get; set; }
}

public class VehicleReportDto
{
    public int TotalVehicles { get; set; }
    public int ServicedVehicles { get; set; }
    public int NeverServicedVehicles { get; set; }
    public List<VehicleMakeReportDto> ByMake { get; set; } = new();
    public List<VehicleFuelTypeReportDto> ByFuelType { get; set; } = new();
}

public class VehicleMakeReportDto
{
    public string Make { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class VehicleFuelTypeReportDto
{
    public string FuelType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class InventoryReportDto
{
    public int TotalActiveParts { get; set; }
    public int TotalQuantity { get; set; }
    public int LowStockParts { get; set; }
    public int OutOfStockParts { get; set; }
    public decimal InventoryValue { get; set; }
    public List<LowStockPartDto> LowStockItems { get; set; } = new();
}

public class LowStockPartDto
{
    public int PartId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsOutOfStock { get; set; }
}
