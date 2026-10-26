namespace AutoCare.Api.Services;

/// <summary>
/// Outcome of attempting to delete a vehicle.
/// </summary>
public enum VehicleDeleteResult
{
    Deleted,
    NotFound,
    HasServiceHistory
}
