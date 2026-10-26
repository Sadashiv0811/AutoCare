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
public class ServiceJobsController : ControllerBase
{
    private readonly ServiceJobService _service;

    public ServiceJobsController(ServiceJobService service)
    {
        _service = service;
    }

    [HttpGet("my")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<List<ServiceJobResponseDto>>> GetMyJobs([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        return Ok(ListQuery.Apply(await _service.GetMyJobsAsync(userId), pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("my/{id:int}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<ServiceJobResponseDto>> GetMyJob(int id)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        var job = await _service.GetMyJobAsync(id, userId);

        if (job == null)
        {
            return NotFound(new { message = "Service job not found." });
        }

        return Ok(job);
    }

    [HttpGet("assigned")]
    [Authorize(Roles = Roles.Mechanic)]
    public async Task<ActionResult<List<ServiceJobResponseDto>>> GetAssignedJobs([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        return Ok(ListQuery.Apply(await _service.GetAssignedJobsAsync(userId), pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("assigned/{id:int}")]
    [Authorize(Roles = Roles.Mechanic)]
    public async Task<ActionResult<ServiceJobResponseDto>> GetAssignedJob(int id)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        var job = await _service.GetAssignedJobAsync(id, userId);

        if (job == null)
        {
            return NotFound(new { message = "Service job not found." });
        }

        return Ok(job);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Mechanic)]
    public async Task<ActionResult<ServiceJobResponseDto>> UpdateStatus(
        int id,
        ServiceJobStatusUpdateDto dto)
    {
        if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId))
        {
            return Unauthorized();
        }

        ServiceJobResponseDto? job;
        try
        {
            job = await _service.UpdateStatusAsync(id, userId, dto.Status);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (job == null)
        {
            return NotFound(new
            {
                message = "Service job not found or not assigned to you."
            });
        }

        return Ok(job);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<List<ServiceJobResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var serviceJobs = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(serviceJobs, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<ServiceJobResponseDto>> GetById(
        int id)
    {
        var serviceJob = await _service.GetByIdAsync(id);

        if (serviceJob == null)
        {
            return NotFound(new
            {
                message = "Service job not found."
            });
        }

        return Ok(serviceJob);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<ServiceJobResponseDto>> Create(
        ServiceJobCreateDto dto)
    {
        var serviceJob =
            await _service.CreateAsync(dto);

        if (serviceJob == null)
        {
            return BadRequest(new
            {
                message =
                    "Vehicle, mechanic, or service type not found."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = serviceJob.Id },
            serviceJob);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
    public async Task<ActionResult<ServiceJobResponseDto>> Update(
        int id,
        ServiceJobUpdateDto dto)
    {
        ServiceJobResponseDto? serviceJob;
        try
        {
            serviceJob = await _service.UpdateAsync(id, dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (serviceJob == null)
        {
            return NotFound(new
            {
                message =
                    "Service job not found, or a related record does not exist."
            });
        }

        return Ok(serviceJob);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
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
                message = "Service job not found."
            });
        }

        return NoContent();
    }
}