using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class MaintenanceRecordCreateDto
{
    [Range(1, int.MaxValue)]

    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]

    public int? ServiceJobId { get; set; }

    [Required]

    [StringLength(150, MinimumLength = 2)]

    public string ServiceName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal Cost { get; set; }

    [Required]

    public DateTime ServiceDate { get; set; }

    [StringLength(4000)]

    public string? Notes { get; set; }
}

public class MaintenanceRecordResponseDto
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public string VehicleRegistrationNumber { get; set; } =
        string.Empty;

    public int? ServiceJobId { get; set; }

    public string? JobNumber { get; set; }

    public string ServiceName { get; set; } =
        string.Empty;

    public decimal Mileage { get; set; }

    public decimal Cost { get; set; }

    public DateTime ServiceDate { get; set; }

    public string? Notes { get; set; }
}

public class MaintenanceRecordUpdateDto
{
    [Range(1, int.MaxValue)]

    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]

    public int? ServiceJobId { get; set; }

    [Required]

    [StringLength(150, MinimumLength = 2)]

    public string ServiceName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]

    public decimal Cost { get; set; }

    [Required]

    public DateTime ServiceDate { get; set; }

    [StringLength(4000)]

    public string? Notes { get; set; }
}