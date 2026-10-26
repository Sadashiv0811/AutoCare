using AutoCare.Api.Infrastructure;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Mvc;
using AutoCare.Api.Constants;
using Microsoft.AspNetCore.Authorization;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<UserResponseDto>>
    > GetAll([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] bool? isActive = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null)
    {
        var users =
            await _service.GetAllAsync();

        return Ok(ListQuery.Apply(users, pageNumber, pageSize, search, status, isActive, sortBy, sortDirection));
    }

    [HttpGet("{id:int}")]
    public async Task<
        ActionResult<UserResponseDto>
    > GetById(int id)
    {
        var user =
            await _service.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<
        ActionResult<UserResponseDto>
    > Create(UserCreateDto dto)
    {
        var result =
            await _service.CreateAsync(dto);

        if (result.User == null)
        {
            return BadRequest(new
            {
                message = result.Error
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.User.Id },
            result.User);
    }

    [HttpPut("{id:int}")]
    public async Task<
        ActionResult<UserResponseDto>
    > Update(
        int id,
        UserUpdateDto dto)
    {
        var result =
            await _service.UpdateAsync(id, dto);

        if (result.User == null)
        {
            if (result.Error == "User not found.")
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

        return Ok(result.User);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}