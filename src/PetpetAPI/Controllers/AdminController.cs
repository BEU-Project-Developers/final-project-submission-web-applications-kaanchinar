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

    // Review Management Endpoints
    
    /// <summary>
    /// Get paginated list of reviews with filtering and sorting options
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <returns>Paginated list of reviews</returns>
    [HttpGet("reviews")]
    public async Task<ActionResult<ApiResponse<PagedReviewsDto>>> GetReviews([FromQuery] AdminReviewFilterDto filter)
    {
        var result = await _adminService.GetReviewsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get review statistics for admin dashboard
    /// </summary>
    /// <returns>Review statistics including counts, ratings, and trends</returns>
    [HttpGet("reviews/stats")]
    public async Task<ActionResult<ApiResponse<ReviewStatsDto>>> GetReviewStats()
    {
        var result = await _adminService.GetReviewStatsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about a specific review
    /// </summary>
    /// <param name="id">Review ID</param>
    /// <returns>Detailed review information</returns>
    [HttpGet("reviews/{id}")]
    public async Task<ActionResult<ApiResponse<AdminReviewDto>>> GetReviewDetails(int id)
    {
        var result = await _adminService.GetReviewDetailsAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Moderate a single review (approve, reject, or delete)
    /// </summary>
    /// <param name="action">Review moderation action</param>
    /// <returns>Success status</returns>
    [HttpPost("reviews/moderate")]
    public async Task<ActionResult<ApiResponse<bool>>> ModerateReview([FromBody] AdminReviewActionDto action)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.ModerateReviewAsync(action);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Moderate multiple reviews at once (bulk action)
    /// </summary>
    /// <param name="action">Bulk review moderation action</param>
    /// <returns>Success status</returns>
    [HttpPost("reviews/moderate/bulk")]
    public async Task<ActionResult<ApiResponse<bool>>> BulkModerateReviews([FromBody] BulkReviewActionDto action)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _adminService.BulkModerateReviewsAsync(action);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Delete a specific review (admin only)
    /// </summary>
    /// <param name="id">Review ID to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("reviews/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReview(int id)
    {
        // Add detailed logging for debugging
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        Console.WriteLine($"[DEBUG] Admin delete request: ReviewId={id}, AdminUserId={userId}, AdminEmail={userEmail}");
        
        if (id <= 0)
        {
            Console.WriteLine($"[DEBUG] Invalid review ID: {id}");
            return BadRequest(new ApiResponse<bool>
            {
                Success = false,
                Message = "Invalid review ID. ID must be a positive integer."
            });
        }

        var action = new AdminReviewActionDto
        {
            ReviewId = id,
            Action = "delete",
            Reason = "Deleted by admin"
        };

        Console.WriteLine($"[DEBUG] Calling ModerateReviewAsync with action: {action.Action}, ReviewId: {action.ReviewId}");
        var result = await _adminService.ModerateReviewAsync(action);
        Console.WriteLine($"[DEBUG] ModerateReviewAsync result: Success={result.Success}, Message={result.Message}");
        
        if (!result.Success)
        {
            return NotFound(result);
        }
        
        return Ok(result);
    }
}
