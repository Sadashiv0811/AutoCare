using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class RegisterDto
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
}