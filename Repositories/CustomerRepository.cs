using AutoCare.Api.Constants;
using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class CustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.Vehicles)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Vehicles)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByUserIdAsync(int userId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Customer?> GetTrackedByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> CustomerExistsForUserAsync(int userId)
    {
        return await _context.Customers
            .AnyAsync(c => c.UserId == userId);
    }

    public async Task<User?> GetCustomerUserAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                u.Role == Roles.Customer);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);

        await _context.SaveChangesAsync();

        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Id == customer.UserId);

        if (user != null)
        {

            user.FullName = customer.FullName.Trim();
            user.Email = customer.Email ?? "";

            // Update user
            _context.Users.Update(user);

            // Update customer
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Customer customer)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == customer.UserId);

        // Delete customer
        _context.Customers.Remove(customer);

        if (user != null)
        {
            // Delete user
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }
}