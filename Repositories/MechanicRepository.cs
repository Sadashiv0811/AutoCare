using AutoCare.Api.Data;
using AutoCare.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoCare.Api.Repositories;

public class MechanicRepository
{
    private readonly AppDbContext _context;

    public MechanicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mechanic>> GetAllAsync()
    {
        return await _context.Mechanics
            .Include(m => m.ServiceJobs)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Mechanic?> GetByIdAsync(int id)
    {
        return await _context.Mechanics
            .Include(m => m.ServiceJobs)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Mechanic?> GetTrackedByIdAsync(int id)
    {
        return await _context.Mechanics
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> HasAssignedServiceJobsAsync(int mechanicId)
    {
        return await _context.ServiceJobs
            .AnyAsync(job => job.MechanicId == mechanicId);
    }

    public async Task<User?> GetMechanicUserAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == userId &&
                u.Role == AutoCare.Api.Constants.Roles.Mechanic);
    }

    public async Task<bool> MechanicExistsForUserAsync(int userId)
    {
        return await _context.Mechanics
            .AnyAsync(m => m.UserId == userId);
    }

    public async Task<bool> UserEmailExistsAsync(
    string email,
    int excludeUserId)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Email == email &&
                u.Id != excludeUserId);
    }

    public async Task<Mechanic> CreateAsync(Mechanic mechanic)
    {
        _context.Mechanics.Add(mechanic);

        await _context.SaveChangesAsync();

        return mechanic;
    }

    public async Task UpdateAsync(Mechanic mechanic)
    {
        var user = await _context.Users
         .FirstOrDefaultAsync(u => u.Id == mechanic.UserId);

        if (user != null)
        {

            user.FullName = mechanic.FullName.Trim();
            user.Email = mechanic.Email ?? "";

            // Update user
            _context.Users.Update(user);

            // Update mechanic
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Mechanic mechanic)
    {
        var user = await _context.Users
          .FirstOrDefaultAsync(u => u.Id == mechanic.UserId);

        _context.Mechanics.Remove(mechanic);

        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
    }
}