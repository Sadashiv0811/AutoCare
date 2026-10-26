namespace AutoCare.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int? ServiceJobId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public decimal Mileage { get; set; }

    public decimal Cost { get; set; }

    public DateTime ServiceDate { get; set; }

    public string? Notes { get; set; }

    // Navigation properties

    public Vehicle Vehicle { get; set; } = null!;

    public ServiceJob? ServiceJob { get; set; }
}