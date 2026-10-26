using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class ServiceTypeCreateDto
{
    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]

    public string? Description { get; set; }

    [Range(1, 10080)]

    public int EstimatedDurationMinutes { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ServiceTypeResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }

    public int ServiceJobCount { get; set; }
}

public class ServiceTypeUpdateDto
{
    [Required]

    [StringLength(100, MinimumLength = 2)]

    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]

    public string? Description { get; set; }

    [Range(1, 10080)]

    public int EstimatedDurationMinutes { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; }
}