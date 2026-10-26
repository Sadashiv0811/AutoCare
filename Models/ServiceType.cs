namespace AutoCare.Models;

public class ServiceType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<ServiceJob> ServiceJobs { get; set; } =
        new List<ServiceJob>();
}

// Examples:
// Oil Change
// Brake Service
// Engine Service
// AC Service