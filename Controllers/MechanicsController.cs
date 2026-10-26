using AutoCare.Api.Infrastructure;
using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MechanicsController : ControllerBase
{
    private readonly MechanicService _service;

    public MechanicsController(MechanicService service)
    {
        _service = service;
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    [HttpGet]
    public async Task<ActionResult<List<MechanicResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var mechanics = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(mechanics, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MechanicResponseDto>> GetById(int id)
    {
        var mechanic = await _service.GetByIdAsync(id);

        if (mechanic == null)
        {
            return NotFound(new
            {
                message = "Mechanic not found."
            });
        }

        return Ok(mechanic);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<ActionResult<MechanicResponseDto>> Create(
        MechanicCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (result.Mechanic == null)
        {
            return BadRequest(new
            {
                message = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Mechanic.Id },
            result.Mechanic);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MechanicResponseDto>> Update(
      int id,
      MechanicUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);

        if (result.Mechanic == null)
        {
            if (result.Error == "Mechanic not found.")
            {
                return NotFound(new
                {
                    message = result.Error
                });
            }

            return BadRequest(new
            {
                message = result.Error
            });
        }

        return Ok(result.Mechanic);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result.Deleted)
        {
            if (result.Error == "Mechanic not found.")
            {
                return NotFound(new
                {
                    message = result.Error
                });
            }

            return Conflict(new
            {
                message = result.Error
            });
        }

        return NoContent();
    }
}