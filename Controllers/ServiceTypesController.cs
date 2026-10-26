using AutoCare.Api.Infrastructure;
using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceTypesController : ControllerBase
{
    private readonly ServiceTypeService _service;

    public ServiceTypesController(ServiceTypeService service)
    {
        _service = service;
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic + "," +
                Roles.Customer)]
    [HttpGet]
    public async Task<ActionResult<List<ServiceTypeResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var serviceTypes = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(serviceTypes, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor + "," +
                Roles.Mechanic + "," +
                Roles.Customer)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceTypeResponseDto>> GetById(int id)
    {
        var serviceType = await _service.GetByIdAsync(id);

        if (serviceType == null)
        {
            return NotFound(new
            {
                message = "Service type not found."
            });
        }

        return Ok(serviceType);
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor)]
    [HttpPost]
    public async Task<ActionResult<ServiceTypeResponseDto>> Create(
            ServiceTypeCreateDto dto)
    {
        var serviceType = await _service.CreateAsync(dto);

        if (serviceType == null)
        {
            return BadRequest(new
            {
                message = "Invalid service type details or a service type with this name already exists."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = serviceType.Id },
            serviceType);
    }

    [Authorize(
        Roles = Roles.Admin + "," +
                Roles.ServiceAdvisor)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceTypeResponseDto>> Update(
            int id,
            ServiceTypeUpdateDto dto)
    {
        var serviceType =
            await _service.UpdateAsync(id, dto);

        if (serviceType == null)
        {
            return NotFound(new
            {
                message = "Service type not found."
            });
        }

        return Ok(serviceType);
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
                message = "Service type was not found or is used by existing service jobs. Mark it inactive instead of deleting it."
            });
        }

        return NoContent();
    }
}