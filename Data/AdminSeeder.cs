using AutoCare.Api.Constants;
using AutoCare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IConfiguration configuration)
    {
        // Check whether an Admin already exists
        var adminExists = await context.Users
            .AnyAsync(u => u.Role == Roles.Admin);

        if (adminExists)
        {
            return;
        }

        var adminSeed = configuration.GetSection("AdminSeed");

        var fullName = adminSeed["FullName"];
        var email = adminSeed["Email"];
        var password = adminSeed["Password"];

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "AdminSeed configuration is missing or incomplete.");
        }

        email = email.Trim().ToLowerInvariant();

        // Make sure the email is not already used
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            if (existingUser.Role == Roles.Admin)
            {
                return;
            }

            throw new InvalidOperationException(
                $"The AdminSeed email '{email}' is already used by another user.");
        }

        var admin = new User
        {
            FullName = fullName.Trim(),
            Email = email,
            Role = Roles.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();

        admin.PasswordHash = passwordHasher.HashPassword(
            admin,
            password);

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}