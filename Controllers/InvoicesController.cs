using AutoCare.Api.Infrastructure;
using System.Security.Claims;
using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _service;
    public InvoicesController(InvoiceService service) => _service = service;

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<InvoiceResponseDto>>> GetMyInvoices([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        return Ok(ListQuery.Apply(await _service.GetMyInvoicesAsync(userId), pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("my/{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<InvoiceResponseDto>> GetMyInvoice(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var invoice = await _service.GetMyInvoiceAsync(id, userId);
        return invoice == null ? NotFound(new { message = "Invoice not found." }) : Ok(invoice);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<List<InvoiceResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null) => Ok(ListQuery.Apply(await _service.GetAllAsync(), pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<InvoiceResponseDto>> GetById(int id)
    {
        var invoice = await _service.GetByIdAsync(id);
        return invoice == null ? NotFound(new { message = "Invoice not found." }) : Ok(invoice);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<InvoiceResponseDto>> Create(InvoiceCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        if (result.Invoice == null)
        {
            var error = new { message = result.ErrorMessage ?? "Invoice could not be created." };
            return result.Conflict ? Conflict(error) : BadRequest(error);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Invoice.Id }, result.Invoice);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<InvoiceResponseDto>> Update(int id, InvoiceUpdateDto dto)
    {
        try
        {
            var invoice = await _service.UpdateAsync(id, dto);
            return invoice == null ? NotFound(new { message = "Invoice not found." }) : Ok(invoice);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = "Invoice not found." });
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}
