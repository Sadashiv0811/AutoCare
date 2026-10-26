using System.ComponentModel.DataAnnotations;

namespace AutoCare.Api.DTOs;

public class InvoiceCreateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Service charges must be between 0 and 99,999,999.99.")]
    public decimal ServiceCharges { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal PartsAmount { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Tax amount must be between 0 and 99,999,999.99.")]
    public decimal TaxAmount { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidAt { get; set; }
}

public class InvoiceResponseDto
{
    public int Id { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public int ServiceJobId { get; set; }

    public string JobNumber { get; set; } = string.Empty;

    public decimal ServiceCharges { get; set; }

    public decimal PartsAmount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class InvoiceUpdateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Service charges must be between 0 and 99,999,999.99.")]
    public decimal ServiceCharges { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal PartsAmount { get; set; }
    
    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Tax amount must be between 0 and 99,999,999.99.")]
    public decimal TaxAmount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidAt { get; set; }
}