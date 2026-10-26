using AutoCare.Api.Infrastructure;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Mvc;
using AutoCare.Api.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _service;

    public CustomersController(CustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerResponseDto>>> GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var customers = await _service.GetAllAsync();

        return Ok(ListQuery.Apply(customers, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
    {
        var customer = await _service.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create(
     CustomerCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (result.Customer == null)
        {
            return BadRequest(new
            {
                message = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Customer.Id },
            result.Customer);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(
        int id,
        CustomerUpdateDto dto)
    {
        var customer = await _service.UpdateAsync(id, dto);

        if (customer == null)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        return Ok(customer);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Customer not found."
            });
        }

        return NoContent();
    }
}