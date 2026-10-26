namespace AutoCare.Models;

public class Invoice
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public int ServiceJobId { get; set; }

    public decimal ServiceCharges { get; set; }

    public decimal PartsAmount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property

    public ServiceJob ServiceJob { get; set; } = null!;
}