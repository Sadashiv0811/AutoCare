using System.ComponentModel.DataAnnotations;
namespace AutoCare.Api.DTOs;

public class ServiceJobPartCreateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(1, int.MaxValue)]

    public int PartId { get; set; }

    [Range(1, int.MaxValue)]

    public int Quantity { get; set; }
}

public class ServiceJobPartResponseDto
{
    public int Id { get; set; }

    public int ServiceJobId { get; set; }

    public string JobNumber { get; set; } = string.Empty;

    public int PartId { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string PartName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}

public class ServiceJobPartUpdateDto
{
    [Range(1, int.MaxValue)]

    public int ServiceJobId { get; set; }

    [Range(1, int.MaxValue)]

    public int PartId { get; set; }

    [Range(1, int.MaxValue)]

    public int Quantity { get; set; }
}