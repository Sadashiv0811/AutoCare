using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoCare.Api.Constants;
using AutoCare.Api.Data;
using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AutoCare.Api.Services;

public class AuthService
{
    private readonly UserRepository _repository;
    private readonly CustomerRepository _customerRepository;
    private readonly AppDbContext _context;

    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
      UserRepository repository,
      CustomerRepository customerRepository,
      AppDbContext context,
      IConfiguration configuration)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _context = context;
        _configuration = configuration;

        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<(bool Success, string? Error, int? CustomerId)> RegisterAsync(
      RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return (false, "Full name is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return (false, "Email is required.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return (false, "Password is required.", null);
        }

        var email = dto.Email.Trim().ToLowerInvariant();

        var existingUser =
            await _repository.GetByEmailAsync(email);

        if (existingUser != null)
        {
            return (
                false,
                "A user with this email already exists.", null);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // =========================
            // Create User
            // =========================

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,

                // Public registration always creates Customer
                Role = Roles.Customer,

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    dto.Password);

            await _repository.CreateAsync(user);


            // =========================
            // Create Customer
            // =========================

            var customer = new Customer
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };

            await _customerRepository.CreateAsync(customer);


            // =========================
            // Commit Transaction
            // =========================

            await transaction.CommitAsync();

            return (true, null, customer.Id);
        }
        catch
        {
            // =========================
            // Rollback Transaction
            // =========================

            await transaction.RollbackAsync();

            return (
                false,
                "Registration failed. Please try again.", null);
        }
    }

    public async Task<
         (AuthResponseDto? Response, string? Error)
     > LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return (
                null,
                "Email and password are required.");
        }

        var email =
            dto.Email.Trim().ToLowerInvariant();

        var user =
            await _repository.GetByEmailAsync(email);

        if (user == null)
        {
            return (
                null,
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return (
                null,
                "This account is inactive.");
        }

        var passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

        if (passwordResult ==
            PasswordVerificationResult.Failed)
        {
            return (
                null,
                "Invalid email or password.");
        }

        var token = GenerateJwtToken(user);

        var response = new AuthResponseDto
        {
            Id = user.Id,

            FullName = user.FullName,

            Email = user.Email,

            Role = user.Role,

            Token = token
        };

        return (response, null);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSection =
            _configuration.GetSection("Jwt");

        var key =
            jwtSection["Key"]
            ?? throw new InvalidOperationException(
                "JWT Key is missing.");

        var issuer =
            jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is missing.");

        var audience =
            jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is missing.");

        var expirationMinutes =
            int.Parse(
                jwtSection["ExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,

                audience: audience,

                claims: claims,

                expires:
                    DateTime.UtcNow.AddMinutes(
                        expirationMinutes),

                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}