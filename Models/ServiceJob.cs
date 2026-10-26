namespace AutoCare.Models;

public class ServiceJob
{
    public int Id { get; set; }

    public string JobNumber { get; set; } = string.Empty;

    // Foreign Key
    public int VehicleId { get; set; }

    // Foreign Key
    public int MechanicId { get; set; }
    
    // Foreign Key
    public int ServiceTypeId { get; set; }

    public decimal Mileage { get; set; }

    public string? ProblemDescription { get; set; }

    public string? InspectionNotes { get; set; }

    public ServiceJobStatus Status { get; set; } = ServiceJobStatus.Scheduled;

    public DateTime ScheduledDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    public Vehicle Vehicle { get; set; } = null!;

    public Mechanic Mechanic { get; set; } = null!;

    public ServiceType ServiceType { get; set; } = null!;

    public ICollection<ServiceJobPart> ServiceJobParts { get; set; } =
        new List<ServiceJobPart>();

    public ICollection<ServiceNote> ServiceNotes { get; set; } =
        new List<ServiceNote>();

    public Invoice? Invoice { get; set; }
}

public enum ServiceJobStatus
{
    Scheduled,      // 0
    Received,       // 1
    Inspection,     // 2
    InProgress,     // 3
    Completed,      // 4
    ReadyForPickup, // 5
    Delivered       // 6
}