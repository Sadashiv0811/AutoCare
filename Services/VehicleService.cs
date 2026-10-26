using AutoCare.Api.DTOs;
using AutoCare.Api.Repositories;
using AutoCare.Models;

namespace AutoCare.Api.Services;

public class VehicleService
{
    private readonly VehicleRepository _repository;
    private readonly CustomerRepository _customerRepository;

    public VehicleService(
     VehicleRepository repository,
     CustomerRepository customerRepository)
    {
        _repository = repository;
        _customerRepository = customerRepository;
    }
    public async Task<List<VehicleResponseDto>> GetAllAsync()
    {
        var vehicles = await _repository.GetAllAsync();

        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<VehicleResponseDto?> GetByIdAsync(int id)
    {
        var vehicle = await _repository.GetByIdAsync(id);

        return vehicle == null
            ? null
            : MapToResponse(vehicle);
    }

    public async Task<List<VehicleResponseDto>> GetMyVehiclesAsync(
        int userId)
    {
        var customer =
            await _customerRepository.GetByUserIdAsync(userId);

        if (customer == null)
        {
            return new List<VehicleResponseDto>();
        }

        var vehicles =
            await _repository.GetByCustomerIdAsync(customer.Id);

        return vehicles
            .Select(MapToResponse)
            .ToList();
    }
    public async Task<VehicleResponseDto?> GetMyVehicleAsync(
    int userId,
    int vehicleId)
    {
        var customer =
            await _customerRepository.GetByUserIdAsync(userId);

        if (customer == null)
        {
            return null;
        }

        var vehicle =
            await _repository.GetByIdAndCustomerIdAsync(
                vehicleId,
                customer.Id);

        return vehicle == null
            ? null
            : MapToResponse(vehicle);
    }
    public async Task<VehicleServiceHistoryDto?> GetServiceHistoryAsync(int vehicleId)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);
        if (vehicle == null)
            return null;

        var jobs = await _repository.GetServiceJobsAsync(vehicleId);
        var records = await _repository.GetMaintenanceRecordsAsync(vehicleId);

        var history = jobs.Select(j => new VehicleServiceHistoryItemDto
        {
            ServiceJobId = j.Id,
            JobNumber = j.JobNumber,
            ServiceName = j.ServiceType?.Name ?? "Service",
            Mileage = j.Mileage,
            ServiceDate = j.CompletedDate ?? j.ScheduledDate,
            Cost = records.FirstOrDefault(r => r.ServiceJobId == j.Id)?.Cost ?? 0,
            Status = j.Status.ToString()
        }).ToList();

        foreach (var record in records.Where(r => r.ServiceJobId == null))
        {
            history.Add(new VehicleServiceHistoryItemDto
            {
                ServiceJobId = null,
                JobNumber = null,
                ServiceName = record.ServiceName,
                Mileage = record.Mileage,
                ServiceDate = record.ServiceDate,
                Cost = record.Cost,
                Status = "Historical"
            });
        }

        return new VehicleServiceHistoryDto
        {
            VehicleId = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            History = history.OrderByDescending(h => h.ServiceDate).ToList()
        };
    }

    public async Task<MaintenanceDueDto?> GetMaintenanceDueAsync(int vehicleId)
    {
        var vehicle = await _repository.GetByIdAsync(vehicleId);
        if (vehicle == null)
            return null;

        var records = await _repository.GetMaintenanceRecordsAsync(vehicleId);
        var last = records.FirstOrDefault();

        if (last == null)
        {
            return new MaintenanceDueDto
            {
                VehicleId = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                IsDue = true,
                Message = "Maintenance is due because no maintenance record exists."
            };
        }

        // Default business rule for Phase 6:
        // maintenance is due after 5,000 km or 6 months, whichever comes first.
        var dueMileage = last.Mileage + 5000;
        var dueDate = last.ServiceDate.AddMonths(6);
        var isDue = vehicle.Mileage >= dueMileage || DateTime.UtcNow >= dueDate;

        return new MaintenanceDueDto
        {
            VehicleId = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            IsDue = isDue,
            DueDate = dueDate,
            DueMileage = dueMileage,
            LastServiceDate = last.ServiceDate,
            LastServiceMileage = last.Mileage,
            Message = isDue
                ? "Maintenance is due."
                : $"Next maintenance is due by {dueDate:yyyy-MM-dd} or {dueMileage:0.##} km."
        };
    }

    public async Task<VehicleResponseDto?> CreateAsync(
        VehicleCreateDto dto)
    {
        var customerExists =
            await _repository.CustomerExistsAsync(dto.CustomerId);

        if (!customerExists)
        {
            return null;
        }

        var vehicle = new Vehicle
        {
            RegistrationNumber = dto.RegistrationNumber.Trim(),
            VIN = dto.VIN.Trim(),
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            FuelType = dto.FuelType.Trim(),
            Mileage = dto.Mileage,
            Color = dto.Color?.Trim(),
            CustomerId = dto.CustomerId
        };

        var createdVehicle = await _repository.CreateAsync(vehicle);

        var vehicleWithCustomer =
            await _repository.GetByIdAsync(createdVehicle.Id);

        return vehicleWithCustomer == null
            ? null
            : MapToResponse(vehicleWithCustomer);
    }

    public async Task<VehicleResponseDto?> UpdateAsync(
        int id,
        VehicleUpdateDto dto)
    {
        var vehicle = await _repository.GetTrackedByIdAsync(id);

        if (vehicle == null)
        {
            return null;
        }

        var customerExists =
            await _repository.CustomerExistsAsync(dto.CustomerId);

        if (!customerExists)
        {
            return null;
        }

        vehicle.RegistrationNumber = dto.RegistrationNumber.Trim();
        vehicle.VIN = dto.VIN.Trim();
        vehicle.Make = dto.Make.Trim();
        vehicle.Model = dto.Model.Trim();
        vehicle.Year = dto.Year;
        vehicle.FuelType = dto.FuelType.Trim();
        vehicle.Mileage = dto.Mileage;
        vehicle.Color = dto.Color?.Trim();
        vehicle.CustomerId = dto.CustomerId;

        await _repository.UpdateAsync(vehicle);

        var updatedVehicle =
            await _repository.GetByIdAsync(id);

        return updatedVehicle == null
            ? null
            : MapToResponse(updatedVehicle);
    }

    public async Task<VehicleDeleteResult> DeleteAsync(int id)
    {
        var vehicle = await _repository.GetTrackedByIdAsync(id);

        if (vehicle == null)
        {
            return VehicleDeleteResult.NotFound;
        }

        // Do not delete a vehicle if it has service jobs or maintenance records.
        // Those records are part of the service history and must not be lost.
        if (await _repository.HasServiceHistoryAsync(id))
        {
            return VehicleDeleteResult.HasServiceHistory;
        }

        await _repository.DeleteAsync(vehicle);

        return VehicleDeleteResult.Deleted;
    }

    private static VehicleResponseDto MapToResponse(
        Vehicle vehicle)
    {
        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            VIN = vehicle.VIN,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            FuelType = vehicle.FuelType,
            Mileage = vehicle.Mileage,
            Color = vehicle.Color,
            CustomerId = vehicle.CustomerId,
            CustomerName = vehicle.Customer?.FullName ?? string.Empty
        };
    }
}