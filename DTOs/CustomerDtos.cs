using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class CustomerCreateDto
{
    [Range(1, int.MaxValue)]

    public int UserId { get; set; }

    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string FullName { get; set; } = string.Empty;

    [Required]

    [StringLength(30, MinimumLength = 5)]

    public string Phone { get; set; } = string.Empty;

    [EmailAddress]

    [StringLength(254)]

    public string? Email { get; set; }

    [StringLength(500)]

    public string? Address { get; set; }
}

public class CustomerResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public int VehicleCount { get; set; }
}

public class CustomerUpdateDto
{
    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string FullName { get; set; } = string.Empty;

    [Required]

    [StringLength(30, MinimumLength = 5)]

    public string Phone { get; set; } = string.Empty;

    [EmailAddress]

    [StringLength(254)]

    public string? Email { get; set; }

    [StringLength(500)]

    public string? Address { get; set; }
}