using AutoCare.Api.Constants;
using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetTrackedByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludeUserId = null)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Email == email &&
                (!excludeUserId.HasValue ||
                 u.Id != excludeUserId.Value));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }


    public async Task<int> CountActiveAdminsAsync()
    {
        return await _context.Users
            .CountAsync(u =>
                u.Role == AutoCare.Api.Constants.Roles.Admin &&
                u.IsActive);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task UpdateAsync(User user)
    {
        if (user.Role == Roles.Customer)
        {
            var customer = await _context.Customers
           .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer != null)
            {
                // Update customer
                customer.FullName = user.FullName.Trim();
                customer.Email = user.Email ?? "";
            }
        }
        else if (user.Role == Roles.Mechanic)
        {
            var mechanic = await _context.Mechanics
           .FirstOrDefaultAsync(m => m.UserId == user.Id);

            if (mechanic != null)
            {
                // Update mechanic
                mechanic.FullName = user.FullName.Trim();
                mechanic.Email = user.Email ?? "";
            }
        }

        // Saves User and Customer or Mechanic
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        if (user.Role == Roles.Customer)
        {
            var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Delete customer
            if (customer != null)
                _context.Customers.Remove(customer);
        }
        else if (user.Role == Roles.Mechanic)
        {
            var mechanic = await _context.Mechanics
            .FirstOrDefaultAsync(m => m.UserId == user.Id);

            // Delete mechanic
            if (mechanic != null)
                _context.Mechanics.Remove(mechanic);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}