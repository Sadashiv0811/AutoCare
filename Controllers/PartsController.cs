using AutoCare.Api.Infrastructure;
using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly PartService _service;

    public PartsController(PartService service)
    {
        _service = service;
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    [HttpGet]
    public async Task<ActionResult<List<PartResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var parts = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(parts, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    [HttpGet("low-stock")]
    public async Task<ActionResult<List<PartResponseDto>>> GetLowStock([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var parts = await _service.GetLowStockAsync();
        return Ok(ListQuery.Apply(parts, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartResponseDto>> GetById(int id)
    {
        var part = await _service.GetByIdAsync(id);

        if (part == null)
        {
            return NotFound(new
            {
                message = "Part not found."
            });
        }

        return Ok(part);
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor)]
    [HttpPost]
    public async Task<ActionResult<PartResponseDto>> Create(
            PartCreateDto dto)
    {
        var part = await _service.CreateAsync(dto);

        if (part == null)
        {
            return BadRequest(new
            {
                message = "Invalid part details or part number already exists. Quantity, minimum stock, and price must be non-negative."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = part.Id },
            part);
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PartResponseDto>> Update(
            int id,
            PartUpdateDto dto)
    {
        var part = await _service.UpdateAsync(id, dto);

        if (part == null)
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Part not found." });
            }

            return BadRequest(new
            {
                message = "Invalid part details or Part Number already exists. Check required fields, field lengths, quantity, minimum stock, and price."
            });
        }

        return Ok(part);
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return Conflict(new
            {
                message = "Part was not found or is assigned to a service job. Remove its service-job assignments before deleting it."
            });
        }

        return NoContent();
    }
}