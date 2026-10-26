using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class UserCreateDto
{
    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string FullName { get; set; } = string.Empty;

    [Required]

    [EmailAddress]

    [StringLength(254)]

    public string Email { get; set; } = string.Empty;

    [Required]

    [StringLength(256, MinimumLength = 8)]

    public string Password { get; set; } = string.Empty;

    [Required]

    [RegularExpression("^(Admin|ServiceAdvisor|Mechanic|Customer)$")]

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UserResponseDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UserUpdateDto
{
    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string FullName { get; set; } = string.Empty;

    [Required]

    [EmailAddress]

    [StringLength(254)]

    public string Email { get; set; } = string.Empty;

    [StringLength(256, MinimumLength = 8)]

    public string? Password { get; set; }

    [Required]

    [RegularExpression("^(Admin|ServiceAdvisor|Mechanic|Customer)$")]

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}