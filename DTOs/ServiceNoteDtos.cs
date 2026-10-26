using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class ServiceNoteCreateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(1, int.MaxValue)]

    public int? MechanicId { get; set; }

    [Required]

    [StringLength(4000, MinimumLength = 1)]

    public string Note { get; set; } = string.Empty;
}

public class ServiceNoteResponseDto
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public string JobNumber { get; set; } = string.Empty;

    public int? MechanicId { get; set; }

    public string? MechanicName { get; set; }

    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class ServiceNoteUpdateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(1, int.MaxValue)]

    public int? MechanicId { get; set; }

    [Required]

    [StringLength(4000, MinimumLength = 1)]

    public string Note { get; set; } = string.Empty;
}