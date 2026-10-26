using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class CustomerService
{
    private readonly CustomerRepository _repository;

    public CustomerService(CustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();

        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        return customer == null
            ? null
            : MapToResponse(customer);
    }

    public async Task<(CustomerResponseDto? Customer, string? Error)> CreateAsync(
    CustomerCreateDto dto)
    {
        var user = await _repository.GetCustomerUserAsync(dto.UserId);

        if (user == null)
        {
            return (
                null,
                "User not found or user does not have the Customer role."
            );
        }

        var customerExists =
            await _repository.CustomerExistsForUserAsync(dto.UserId);

        if (customerExists)
        {
            return (
                null,
                "A Customer profile already exists for this user."
            );
        }

        var customer = new Customer
        {
            UserId = dto.UserId,
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email?.Trim(),
            Address = dto.Address?.Trim()
        };

        var createdCustomer = await _repository.CreateAsync(customer);

        return (MapToResponse(createdCustomer), null);
    }
    public async Task<CustomerResponseDto?> UpdateAsync(
        int id,
        CustomerUpdateDto dto)
    {
        var customer = await _repository.GetTrackedByIdAsync(id);

        if (customer == null)
        {
            return null;
        }

        customer.FullName = dto.FullName.Trim();
        customer.Phone = dto.Phone.Trim();
        customer.Email = dto.Email?.Trim();
        customer.Address = dto.Address?.Trim();

        await _repository.UpdateAsync(customer);

        return MapToResponse(customer);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _repository.GetTrackedByIdAsync(id);

        if (customer == null)
        {
            return false;
        }

        await _repository.DeleteAsync(customer);

        return true;
    }

    private static CustomerResponseDto MapToResponse(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            VehicleCount = customer.Vehicles?.Count ?? 0
        };
    }
}