using System.ComponentModel.DataAnnotations;
using AutoCare.Models;

namespace AutoCare.Api.DTOs;

public class ServiceJobCreateDto
{
    [Range(1, int.MaxValue)]

    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]

    public int MechanicId { get; set; }

    [Range(1, int.MaxValue)]

    public int ServiceTypeId { get; set; }

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [StringLength(2000)]

    public string? ProblemDescription { get; set; }

    [StringLength(4000)]

    public string? InspectionNotes { get; set; }

    [EnumDataType(typeof(AutoCare.Models.ServiceJobStatus))]

    public ServiceJobStatus Status { get; set; } =
        ServiceJobStatus.Scheduled;

    [Required]

    [DataType(DataType.DateTime)]

    public DateTime ScheduledDate { get; set; }
}

public class ServiceJobResponseDto
{
    public int Id { get; set; }

    public string JobNumber { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public string VehicleRegistrationNumber { get; set; } =
        string.Empty;

    public int MechanicId { get; set; }

    public string MechanicName { get; set; } =
        string.Empty;

    public int ServiceTypeId { get; set; }

    public string ServiceTypeName { get; set; } =
        string.Empty;

    public decimal Mileage { get; set; }

    public string? ProblemDescription { get; set; }

    public string? InspectionNotes { get; set; }

    public ServiceJobStatus Status { get; set; }

    public DateTime ScheduledDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int PartCount { get; set; }

    public int NoteCount { get; set; }

    public bool HasInvoice { get; set; }
}

public class ServiceJobUpdateDto
{
    [Range(1, int.MaxValue)]

    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]

    public int MechanicId { get; set; }

    [Range(1, int.MaxValue)]

    public int ServiceTypeId { get; set; }

    [Range(typeof(decimal), "0", "99999999")]

    public decimal Mileage { get; set; }

    [StringLength(2000)]

    public string? ProblemDescription { get; set; }

    [StringLength(4000)]

    public string? InspectionNotes { get; set; }

    public DateTime ScheduledDate { get; set; }
}

public class ServiceJobStatusUpdateDto
{
    [EnumDataType(typeof(AutoCare.Models.ServiceJobStatus))]

    public ServiceJobStatus Status { get; set; }
}
