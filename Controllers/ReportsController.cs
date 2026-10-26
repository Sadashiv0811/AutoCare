using AutoCare.Api.Constants;
using AutoCare.Api.DTOs;
using AutoCare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.ServiceAdvisor)]
public class ReportsController : ControllerBase
{
    private readonly DashboardService _service;

    public ReportsController(DashboardService service)
    {
        _service = service;
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueReportDto>> GetRevenueReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            return Ok(await _service.GetRevenueReportAsync(fromDate, toDate));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("services")]
    public async Task<ActionResult<ServiceReportDto>> GetServiceReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        try
        {
            return Ok(await _service.GetServiceReportAsync(fromDate, toDate));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<VehicleReportDto>> GetVehicleReport()
    {
        return Ok(await _service.GetVehicleReportAsync());
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport()
    {
        return Ok(await _service.GetInventoryReportAsync());
    }
}
