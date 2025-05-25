using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Admin;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Services;

namespace PetpetAPI.Controllers;

/// <summary>
/// Admin dashboard and management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Get dashboard statistics for admin panel
    /// </summary>
    /// <returns>Dashboard statistics including product counts, inventory value, and recent products</returns>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get detailed list of products with low stock
    /// </summary>
    /// <returns>List of products that need restocking</returns>
    [HttpGet("products/low-stock")]
    public async Task<ActionResult<ApiResponse<List<LowStockProductDto>>>> GetLowStockProducts()
    {
        var result = await _adminService.GetLowStockProductsAsync();
        return Ok(result);
    }
}
