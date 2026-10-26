
using AutoCare.Api.DTOs;

namespace AutoCare.Api.Services;

public class InvoiceCreateResult
{
    public InvoiceResponseDto? Invoice { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Conflict { get; set; }

}
