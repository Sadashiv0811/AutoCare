using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class VehicleCreateDto
{
    [Required]

    [StringLength(20, MinimumLength = 2)]

    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]

    [StringLength(17, MinimumLength = 5)]

    public string VIN { get; set; } = string.Empty;

    [Required]

    [StringLength(80)]

    public string Make { get; set; } = string.Empty;

    [Required]

    [StringLength(80)]

    public string Model { get; set; } = string.Empty;

    [Range(1886, 2100)]

    public int Year { get; set; }

    [Required]

    [StringLength(30)]

    public string FuelType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [StringLength(40)]

    public string? Color { get; set; }

    [Range(1, int.MaxValue)]

    public int CustomerId { get; set; }
}

public class VehicleResponseDto
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string VIN { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string FuelType { get; set; } = string.Empty;

    public decimal Mileage { get; set; }

    public string? Color { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;
}

public class VehicleUpdateDto
{
    [Required]

    [StringLength(20, MinimumLength = 2)]

    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]

    [StringLength(17, MinimumLength = 5)]

    public string VIN { get; set; } = string.Empty;

    [Required]

    [StringLength(80)]

    public string Make { get; set; } = string.Empty;

    [Required]

    [StringLength(80)]

    public string Model { get; set; } = string.Empty;

    [Range(1886, 2100)]

    public int Year { get; set; }

    [Required]

    [StringLength(30)]

    public string FuelType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [StringLength(40)]

    public string? Color { get; set; }

    [Range(1, int.MaxValue)]

    public int CustomerId { get; set; }
}