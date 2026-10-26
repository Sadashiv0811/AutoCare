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
public class MaintenanceRecordsController : ControllerBase
{
    private readonly MaintenanceRecordService _service;

    public MaintenanceRecordsController(
        MaintenanceRecordService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<MaintenanceRecordResponseDto>>> GetMyRecords([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        return Ok(ListQuery.Apply(await _service.GetMyRecordsAsync(userId), pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("my/{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MaintenanceRecordResponseDto>> GetMyRecord(int id)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        var record = await _service.GetMyRecordAsync(id, userId);

        if (record == null)
        {
            return NotFound(new { message = "Maintenance record not found." });
        }

        return Ok(record);
    }

    [HttpGet]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<
        ActionResult<List<MaintenanceRecordResponseDto>>
    > GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var records =
            await _service.GetAllAsync();

        return Ok(ListQuery.Apply(records, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<
        ActionResult<MaintenanceRecordResponseDto>
    > GetById(int id)
    {
        var record =
            await _service.GetByIdAsync(id);

        if (record == null)
        {
            return NotFound(new
            {
                message = "Maintenance record not found."
            });
        }

        return Ok(record);
    }

    [HttpPost]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<
        ActionResult<MaintenanceRecordResponseDto>
    > Create(
        MaintenanceRecordCreateDto dto)
    {
        var record =
            await _service.CreateAsync(dto);

        if (record == null)
        {
            return BadRequest(new
            {
                message =
                    "Vehicle not found or service job not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = record.Id },
            record);
    }

    [HttpPut("{id:int}")]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<
        ActionResult<MaintenanceRecordResponseDto>
    > Update(
        int id,
        MaintenanceRecordUpdateDto dto)
    {
        var record =
            await _service.UpdateAsync(id, dto);

        if (record == null)
        {
            return NotFound(new
            {
                message =
                    "Maintenance record not found, " +
                    "or a related record does not exist."
            });
        }

        return Ok(record);
    }

    [HttpDelete("{id:int}")]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Maintenance record not found."
            });
        }

        return NoContent();
    }
}