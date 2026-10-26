using System.ComponentModel.DataAnnotations;
using AutoCare.Models;

namespace AutoCare.Api.DTOs;

public class PartCreateDto
{
    [Required]

    [StringLength(50, MinimumLength = 1)]

    public string PartNumber { get; set; } = string.Empty;

    [Required]

    [StringLength(150, MinimumLength = 2)]

    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]

    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]

    public int MinimumStock { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal UnitPrice { get; set; }

    [StringLength(150)]

    public string? Supplier { get; set; }

    public bool IsActive { get; set; } = true;
}

public class PartResponseDto
{
    public int Id { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public int MinimumStock { get; set; }

    public decimal UnitPrice { get; set; }

    public string? Supplier { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ServiceJobPartCount { get; set; }

    public bool IsLowStock { get; set; }
}

public class PartUpdateDto
{
    [Required]

    [StringLength(50, MinimumLength = 1)]

    public string PartNumber { get; set; } = string.Empty;

    [Required]

    [StringLength(150, MinimumLength = 2)]

    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]

    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]

    public int MinimumStock { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal UnitPrice { get; set; }

    [StringLength(150)]

    public string? Supplier { get; set; }

    public bool IsActive { get; set; }
}