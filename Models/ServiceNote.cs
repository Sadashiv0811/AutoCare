namespace AutoCare.Models;

public class ServiceNote
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public int? MechanicId { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    public ServiceJob ServiceJob { get; set; } = null!;

    public Mechanic? Mechanic { get; set; }
}