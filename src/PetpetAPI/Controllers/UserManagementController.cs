using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Services;

namespace PetpetAPI.Controllers;

/// <summary>
/// Example controller demonstrating authentication helper usage
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserManagementController : ControllerBase
{
    private readonly IAuthenticationHelper _authHelper;

    public UserManagementController(IAuthenticationHelper authHelper)
    {
        _authHelper = authHelper;
    }

    /// <summary>
    /// Get current user information (demonstrates basic authentication)
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<object>>> GetProfile()
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var user = await _authHelper.GetUserByIdAsync(authResult.Data!);
        if (user == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

        var userRoles = await _authHelper.GetUserRolesAsync(authResult.Data!);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Profile retrieved successfully",
            Data = new
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = userRoles
            }
        });
    }

    /// <summary>
    /// Check user permissions (demonstrates role checking)
    /// </summary>
    /// <returns>User permissions information</returns>
    [HttpGet("permissions")]
    public ActionResult<ApiResponse<object>> GetPermissions()
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var permissions = new
        {
            UserId = _authHelper.GetCurrentUserId(User),
            Roles = _authHelper.GetCurrentUserRoles(User),
            IsAdmin = _authHelper.IsAdmin(User),
            HasUserRole = _authHelper.HasRole(User, "User"),
            HasAdminRole = _authHelper.HasRole(User, "Admin")
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Permissions retrieved successfully",
            Data = permissions
        });
    }

    /// <summary>
    /// Admin-only endpoint (demonstrates admin role checking)
    /// </summary>
    /// <returns>Admin dashboard data</returns>
    [HttpGet("admin/dashboard")]
    public ActionResult<ApiResponse<object>> GetAdminDashboard()
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        // Check if user is admin
        if (!_authHelper.IsAdmin(User))
            return Forbid();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Admin dashboard data retrieved successfully",
            Data = new
            {
                Message = "Welcome to admin dashboard",
                AdminUserId = _authHelper.GetCurrentUserId(User),
                AccessLevel = "Administrator"
            }
        });
    }

    /// <summary>
    /// Resource access check (demonstrates resource ownership)
    /// </summary>
    /// <param name="targetUserId">Target user ID to check access for</param>
    /// <returns>Access check result</returns>
    [HttpGet("access-check/{targetUserId}")]
    public ActionResult<ApiResponse<object>> CheckResourceAccess(string targetUserId)
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var canAccess = _authHelper.CanAccessResource(User, targetUserId);
        var currentUserId = _authHelper.GetCurrentUserId(User);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Access check completed",
            Data = new
            {
                CurrentUserId = currentUserId,
                TargetUserId = targetUserId,
                CanAccess = canAccess,
                Reason = canAccess ? 
                    (currentUserId == targetUserId ? "Owner" : "Admin") : 
                    "Access denied - not owner or admin"
            }
        });
    }

    /// <summary>
    /// User status check (demonstrates async user validation)
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>User status information</returns>
    [HttpGet("status/{userId}")]
    public async Task<ActionResult<ApiResponse<object>>> CheckUserStatus(string userId)
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        // Only allow admins or users checking their own status
        if (!_authHelper.CanAccessResource(User, userId))
            return Forbid();

        var user = await _authHelper.GetUserByIdAsync(userId);
        var isActive = await _authHelper.IsUserActiveAsync(userId);
        var userRoles = await _authHelper.GetUserRolesAsync(userId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "User status retrieved successfully",
            Data = new
            {
                UserId = userId,
                Exists = user != null,
                IsActive = isActive,
                Roles = userRoles,
                Email = user?.Email,
                Name = user != null ? $"{user.FirstName} {user.LastName}" : null,
                CreatedAt = user?.CreatedAt
            }
        });
    }
}
