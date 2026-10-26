using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class MechanicCreateDto
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

    [StringLength(100)]

    public string? Specialization { get; set; }

    [Range(0, 80)]

    public int ExperienceYears { get; set; }

    public bool IsActive { get; set; } = true;
}

public class MechanicResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Specialization { get; set; }

    public int ExperienceYears { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int ServiceJobCount { get; set; }
}

public class MechanicUpdateDto
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

    [StringLength(100)]

    public string? Specialization { get; set; }

    [Range(0, 80)]

    public int ExperienceYears { get; set; }

    public bool IsActive { get; set; }
}
