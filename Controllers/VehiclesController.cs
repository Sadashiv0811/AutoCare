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
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _service;

    public VehiclesController(VehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(
    Roles = Roles.Admin + "," +
            Roles.ServiceAdvisor + "," +
            Roles.Mechanic)]
    public async Task<ActionResult<List<VehicleResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var vehicles = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(vehicles, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    [Authorize(
    Roles = Roles.Admin + "," +
            Roles.ServiceAdvisor + "," +
            Roles.Mechanic)]
    public async Task<ActionResult<VehicleResponseDto>> GetById(int id)
    {
        var vehicle = await _service.GetByIdAsync(id);

        if (vehicle == null)
        {
            return NotFound(new
            {
                message = "Vehicle not found."
            });
        }

        return Ok(vehicle);
    }

    [HttpGet("{id:int}/service-history")]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<ActionResult<VehicleServiceHistoryDto>> GetServiceHistory(int id)
    {
        var history = await _service.GetServiceHistoryAsync(id);

        if (history == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        return Ok(history);
    }

    [HttpGet("{id:int}/maintenance-due")]
    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic)]
    public async Task<ActionResult<MaintenanceDueDto>> GetMaintenanceDue(int id)
    {
        var due = await _service.GetMaintenanceDueAsync(id);

        if (due == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        return Ok(due);
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<VehicleResponseDto>>> GetMyVehicles([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var vehicles =
            await _service.GetMyVehiclesAsync(userId);

        return Ok(ListQuery.Apply(vehicles, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("my/{id:int}/service-history")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<VehicleServiceHistoryDto>> GetMyServiceHistory(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var vehicle = await _service.GetMyVehicleAsync(userId, id);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found." });

        var history = await _service.GetServiceHistoryAsync(id);
        return Ok(history);
    }

    [HttpGet("my/{id:int}/maintenance-due")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<MaintenanceDueDto>> GetMyMaintenanceDue(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var vehicle = await _service.GetMyVehicleAsync(userId, id);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found." });

        var due = await _service.GetMaintenanceDueAsync(id);
        return Ok(due);
    }

    [HttpGet("my/{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<VehicleResponseDto>> GetMyVehicle(
        int id)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var vehicle =
            await _service.GetMyVehicleAsync(
                userId,
                id);

        if (vehicle == null)
        {
            return NotFound(new
            {
                message = "Vehicle not found."
            });
        }

        return Ok(vehicle);
    }

    [HttpPost]
    [Authorize(
    Roles = Roles.Admin + "," +
            Roles.ServiceAdvisor + "," +
            Roles.Mechanic)]
    public async Task<ActionResult<VehicleResponseDto>> Create(
        VehicleCreateDto dto)
    {
        var vehicle = await _service.CreateAsync(dto);

        if (vehicle == null)
        {
            return BadRequest(new
            {
                message = "Customer not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = vehicle.Id },
            vehicle);
    }

    [HttpPut("{id:int}")]
    [Authorize(
    Roles = Roles.Admin + "," +
            Roles.ServiceAdvisor + "," +
            Roles.Mechanic)]
    public async Task<ActionResult<VehicleResponseDto>> Update(
        int id,
        VehicleUpdateDto dto)
    {
        var vehicle = await _service.UpdateAsync(id, dto);

        if (vehicle == null)
        {
            return NotFound(new
            {
                message = "Vehicle not found or customer not found."
            });
        }

        return Ok(vehicle);
    }

    [HttpDelete("{id:int}")]
    [Authorize(
    Roles = Roles.Admin + "," +
            Roles.ServiceAdvisor + "," +
            Roles.Mechanic)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (result == VehicleDeleteResult.NotFound)
        {
            return NotFound(new
            {
                message = "Vehicle not found."
            });
        }

        if (result == VehicleDeleteResult.HasServiceHistory)
        {
            return Conflict(new
            {
                message = "This vehicle cannot be deleted because it has service jobs or maintenance records. The vehicle must be retained to preserve its service history."
            });
        }

        return NoContent();
    }
}