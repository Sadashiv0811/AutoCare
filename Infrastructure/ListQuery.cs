using System.Collections;
using System.Reflection;

namespace AutoCare.Api.Infrastructure;

/// <summary>Optional, backwards-compatible list query support. When pageNumber/pageSize are supplied,
/// the response is { items, totalCount, pageNumber, pageSize, totalPages }. Without paging parameters,
/// list endpoints continue returning a JSON array for existing clients.</summary>
public static class ListQuery
{
    private const int MaxPageSize = 100;
    private static readonly Dictionary<Type, string[]> SortFields = new()
    {
        [typeof(AutoCare.Api.DTOs.CustomerResponseDto)] = new[] { "id", "fullName", "phone", "email", "createdAt", "vehicleCount" },
        [typeof(AutoCare.Api.DTOs.VehicleResponseDto)] = new[] { "id", "registrationNumber", "vin", "make", "model", "year", "mileage", "customerName" },
        [typeof(AutoCare.Api.DTOs.MechanicResponseDto)] = new[] { "id", "fullName", "experienceYears", "isActive", "createdAt", "serviceJobCount" },
        [typeof(AutoCare.Api.DTOs.PartResponseDto)] = new[] { "id", "partNumber", "name", "quantity", "minimumStock", "unitPrice", "isActive", "createdAt" },
        [typeof(AutoCare.Api.DTOs.ServiceTypeResponseDto)] = new[] { "id", "name", "estimatedDurationMinutes", "basePrice", "isActive" },
        [typeof(AutoCare.Api.DTOs.ServiceJobResponseDto)] = new[] { "id", "jobNumber", "vehicleRegistrationNumber", "mechanicName", "serviceTypeName", "status", "scheduledDate", "createdAt" },
        [typeof(AutoCare.Api.DTOs.InvoiceResponseDto)] = new[] { "id", "invoiceNumber", "jobNumber", "totalAmount", "isPaid", "createdAt", "paidAt" },
        [typeof(AutoCare.Api.DTOs.MaintenanceRecordResponseDto)] = new[] { "id", "vehicleRegistrationNumber", "serviceName", "mileage", "cost", "serviceDate" },
        [typeof(AutoCare.Api.DTOs.ServiceJobPartResponseDto)] = new[] { "id", "jobNumber", "partNumber", "partName", "quantity", "unitPrice", "totalPrice" },
        [typeof(AutoCare.Api.DTOs.ServiceNoteResponseDto)] = new[] { "id", "jobNumber", "mechanicName", "createdAt" },
        [typeof(AutoCare.Api.DTOs.UserResponseDto)] = new[] { "id", "fullName", "email", "role", "isActive", "createdAt" }
    };

    public static object Apply<T>(IEnumerable<T> source, int? pageNumber = null, int? pageSize = null,
        string? search = null, string? status = null, bool? isActive = null,
        string? sortBy = null, string? sortDirection = null)
    {
        var items = source.ToList();
        var type = typeof(T);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchable = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(string)).ToArray();
            var term = search.Trim();
            items = items.Where(item => item != null && searchable.Any(p =>
                (p.GetValue(item) as string)?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusProperty = type.GetProperty("Status", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (statusProperty == null)
                throw new ArgumentException($"Status filtering is not supported for {type.Name}.");
            items = items.Where(item => string.Equals(Convert.ToString(statusProperty.GetValue(item)), status.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (isActive.HasValue)
        {
            var activeProperty = type.GetProperty("IsActive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (activeProperty == null)
                throw new ArgumentException($"Active filtering is not supported for {type.Name}.");
            items = items.Where(item => activeProperty.GetValue(item) is bool value && value == isActive.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(sortDirection) && string.IsNullOrWhiteSpace(sortBy))
            throw new ArgumentException("sortBy is required when sortDirection is supplied.");

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            if (!SortFields.TryGetValue(type, out var allowed) || !allowed.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Unsupported sortBy field for {type.Name}. Allowed fields: {string.Join(", ", allowed ?? Array.Empty<string>())}.");
            var property = type.GetProperty(sortBy, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)!;
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            items = (descending
                ? items.OrderByDescending(x => property.GetValue(x))
                : items.OrderBy(x => property.GetValue(x))).ToList();
        }
        else if (!string.IsNullOrWhiteSpace(sortDirection) && !new[] { "asc", "desc" }.Contains(sortDirection, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("sortDirection must be 'asc' or 'desc'.");
        }

        if (!pageNumber.HasValue && !pageSize.HasValue) return items;
        var page = pageNumber ?? 1;
        var size = pageSize ?? 10;
        if (page < 1) throw new ArgumentException("pageNumber must be at least 1.");
        if (size < 1 || size > MaxPageSize) throw new ArgumentException($"pageSize must be between 1 and {MaxPageSize}.");
        var total = items.Count;
        var skipLong = ((long)page - 1L) * size;
        var skip = skipLong > int.MaxValue ? int.MaxValue : (int)skipLong;
        return new PagedResult<T>(items.Skip(skip).Take(size).ToList(), total, page, size,
            total == 0 ? 0 : (int)Math.Ceiling(total / (double)size));
    }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages);
