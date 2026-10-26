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
public class ServiceNotesController : ControllerBase
{
    private readonly ServiceNoteService _service;

    public ServiceNotesController(
        ServiceNoteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<ServiceNoteResponseDto>>
    > GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var notes =
            await _service.GetAllAsync();

        return Ok(ListQuery.Apply(notes, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    public async Task<
        ActionResult<ServiceNoteResponseDto>
    > GetById(int id)
    {
        var note =
            await _service.GetByIdAsync(id);

        if (note == null)
        {
            return NotFound(new
            {
                message = "Service note not found."
            });
        }

        return Ok(note);
    }

    [HttpPost]
    public async Task<
        ActionResult<ServiceNoteResponseDto>
    > Create(
        ServiceNoteCreateDto dto)
    {
        var note =
            await _service.CreateAsync(dto);

        if (note == null)
        {
            return BadRequest(new
            {
                message =
                    "Service job not found or mechanic not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = note.Id },
            note);
    }

    [HttpPut("{id:int}")]
    public async Task<
        ActionResult<ServiceNoteResponseDto>
    > Update(
        int id,
        ServiceNoteUpdateDto dto)
    {
        var note =
            await _service.UpdateAsync(id, dto);

        if (note == null)
        {
            return NotFound(new
            {
                message =
                    "Service note not found, " +
                    "or a related record does not exist."
            });
        }

        return Ok(note);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Service note not found."
            });
        }

        return NoContent();
    }
}