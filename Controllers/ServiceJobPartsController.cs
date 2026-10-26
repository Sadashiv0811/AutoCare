using AutoCare.Api.Infrastructure;
using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
public class ServiceJobPartsController : ControllerBase
{
    private readonly ServiceJobPartService _service;

    public ServiceJobPartsController(
        ServiceJobPartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<ServiceJobPartResponseDto>>
    > GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var parts =
            await _service.GetAllAsync();

        return Ok(ListQuery.Apply(parts, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    public async Task<
        ActionResult<ServiceJobPartResponseDto>
    > GetById(int id)
    {
        var part =
            await _service.GetByIdAsync(id);

        if (part == null)
        {
            return NotFound(new
            {
                message =
                    "Service job part not found."
            });
        }

        return Ok(part);
    }

    [HttpPost]
    public async Task<
        ActionResult<ServiceJobPartResponseDto>
    > Create(
        ServiceJobPartCreateDto dto)
    {
        ServiceJobPartResponseDto? part;
        try
        {
            part = await _service.CreateAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        if (part == null)
        {
            return BadRequest(new
            {
                message =
                    "Invalid service job or part, inactive part, " +
                    "non-positive quantity, or insufficient stock."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = part.Id },
            part);
    }

    [HttpPut("{id:int}")]
    public async Task<
        ActionResult<ServiceJobPartResponseDto>
    > Update(
        int id,
        ServiceJobPartUpdateDto dto)
    {
        ServiceJobPartResponseDto? part;
        try
        {
            part = await _service.UpdateAsync(id, dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        if (part == null)
        {
            return BadRequest(new
            {
                message =
                    "Service job part not found, invalid target, " +
                    "inactive part, duplicate assignment, or insufficient stock."
            });
        }

        return Ok(part);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted;
        try
        {
            deleted = await _service.DeleteAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        if (!deleted)
        {
            return NotFound(new
            {
                message =
                    "Service job part not found."
            });
        }

        return NoContent();
    }
}