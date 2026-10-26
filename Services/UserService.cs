using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using Microsoft.AspNetCore.Identity;

namespace AutoCare.Api.Services;

public class UserService
{
    private readonly UserRepository _repository;

    private readonly PasswordHasher<User> _passwordHasher;

    private static readonly string[] AllowedRoles =
    {
        Roles.Admin,
        Roles.ServiceAdvisor,
        Roles.Mechanic,
        Roles.Customer
    };

    public UserService(UserRepository repository)
    {
        _repository = repository;

        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();

        return users
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<UserResponseDto?> GetByIdAsync(
        int id)
    {
        var user = await _repository.GetByIdAsync(id);

        return user == null
            ? null
            : MapToResponse(user);
    }

    public async Task<
        (UserResponseDto? User, string? Error)
    > CreateAsync(UserCreateDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var normalizedRole = NormalizeRole(dto.Role);

        if (normalizedRole == null)
        {
            return (null,
                "Invalid role. Allowed roles are " +
                "Admin, Service Advisor, Mechanic, Customer.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return (null, "Password is required.");
        }

        var emailExists =
            await _repository.EmailExistsAsync(email);

        if (emailExists)
        {
            return (null,
                "A user with this email already exists.");
        }

        var user = new User
        {
            FullName = dto.FullName.Trim(),

            Email = email,

            Role = normalizedRole,

            IsActive = dto.IsActive
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                dto.Password);

        var createdUser =
            await _repository.CreateAsync(user);

        return (MapToResponse(createdUser), null);
    }

    public async Task<
        (UserResponseDto? User, string? Error)
    > UpdateAsync(
        int id,
        UserUpdateDto dto)
    {
        var user =
            await _repository.GetTrackedByIdAsync(id);

        if (user == null)
        {
            return (null, "User not found.");
        }

        var normalizedRole = NormalizeRole(dto.Role);

        if (normalizedRole == null)
        {
            return (null,
                "Invalid role. Allowed roles are " +
                "Admin, Service Advisor, Mechanic, Customer.");
        }


        if (user.Role == Roles.Admin &&
            user.IsActive)
        {
            var activeAdminCount =
                await _repository.CountActiveAdminsAsync();

            if (user.IsActive && activeAdminCount <= 1)
            {
                return (null,
                    "The last active Admin cannot be deactivated or changed to another role.");
            }
        }

        var email =
            dto.Email.Trim().ToLowerInvariant();

        var emailExists =
            await _repository.EmailExistsAsync(
                email,
                id);

        if (emailExists)
        {
            return (null,
                "A user with this email already exists.");
        }

        user.FullName =
            dto.FullName.Trim();

        user.Email =
            email;

        user.Role =
            normalizedRole;

        user.IsActive =
            dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    dto.Password);
        }

        await _repository.UpdateAsync(user);

        return (MapToResponse(user), null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user =
            await _repository.GetTrackedByIdAsync(id);

        if (user == null)
        {
            return false;
        }

        // Console.WriteLine(
        //     $"DELETE DEBUG: Id={user.Id}, " +
        //     $"Role='{user.Role}', " +
        //     $"IsActive={user.IsActive}");

        if (user.Role == Roles.Admin && user.IsActive)
        {
            var activeAdminCount =
                await _repository.CountActiveAdminsAsync();

            // Console.WriteLine(
            //     $"DELETE DEBUG: Active Admin Count={activeAdminCount}");

            if (activeAdminCount <= 1)
            {
                // Console.WriteLine(
                //     "DELETE DEBUG: Blocking deletion of last Admin.");

                throw new InvalidOperationException(
                    "The last active Admin cannot be deleted.");
            }
        }

        // Console.WriteLine(
        //     "DELETE DEBUG: Proceeding to repository DeleteAsync.");

        await _repository.DeleteAsync(user);

        return true;
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return AllowedRoles.FirstOrDefault(
            allowedRole =>
                string.Equals(
                    allowedRole,
                    role.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    private static UserResponseDto MapToResponse(
        User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,

            FullName = user.FullName,

            Email = user.Email,

            Role = user.Role,

            IsActive = user.IsActive,

            CreatedAt = user.CreatedAt
        };
    }
}