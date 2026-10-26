namespace AutoCare.Models;

public class ServiceJobPart
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public int PartId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    // Navigation properties
    public ServiceJob ServiceJob { get; set; } = null!;

    public Part Part { get; set; } = null!;
}