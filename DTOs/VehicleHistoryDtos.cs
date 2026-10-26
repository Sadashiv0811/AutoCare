namespace AutoCare.Api.DTOs;

public class VehicleServiceHistoryDto
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public List<VehicleServiceHistoryItemDto> History { get; set; } = new();
}

public class VehicleServiceHistoryItemDto
{
    public int? ServiceJobId { get; set; }
    public string? JobNumber { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Mileage { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal Cost { get; set; }
    public string? Status { get; set; }
}

public class MaintenanceDueDto
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public bool IsDue { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? DueMileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public decimal? LastServiceMileage { get; set; }
    public string Message { get; set; } = string.Empty;
}
