namespace AutoCare.Models;

public class Vehicle
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string VIN { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string FuelType { get; set; } = string.Empty;

    public decimal Mileage { get; set; }

    public string? Color { get; set; }

    // Foreign key
    public int CustomerId { get; set; }

    // Navigation property
    public Customer Customer { get; set; } = null!;

    public ICollection<ServiceJob> ServiceJobs { get; set; } = new List<ServiceJob>();

    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } =
        new List<MaintenanceRecord>();
}