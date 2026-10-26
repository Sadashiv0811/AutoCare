namespace AutoCare.Models;

public class Part
{
    public int Id { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public int MinimumStock { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Supplier { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<ServiceJobPart> ServiceJobParts { get; set; } =
        new List<ServiceJobPart>();
}

// This allows us to detect:
// Quantity = 3
// MinimumStock = 10

// LOW STOCK