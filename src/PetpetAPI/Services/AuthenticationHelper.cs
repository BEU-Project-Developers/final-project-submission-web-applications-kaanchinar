using Microsoft.AspNetCore.Identity;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Models;
using System.Security.Claims;

namespace PetpetAPI.Services;

/// <summary>
/// Authentication helper service for managing user session checks and role validation
/// </summary>
public interface IAuthenticationHelper
{
    /// <summary>
    /// Get the current user ID from ClaimsPrincipal
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <returns>User ID or null if not found</returns>
    string? GetCurrentUserId(ClaimsPrincipal user);

    /// <summary>
    /// Get all roles for the current user
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <returns>List of user roles</returns>
    List<string> GetCurrentUserRoles(ClaimsPrincipal user);

    /// <summary>
    /// Check if the current user has a specific role
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <param name="role">Role to check for</param>
    /// <returns>True if user has the role</returns>
    bool HasRole(ClaimsPrincipal user, string role);

    /// <summary>
    /// Check if the current user is an admin
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <returns>True if user is admin</returns>
    bool IsAdmin(ClaimsPrincipal user);

    /// <summary>
    /// Check if the current user owns a resource or is an admin
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <param name="resourceUserId">The user ID that owns the resource</param>
    /// <returns>True if user owns the resource or is admin</returns>
    bool CanAccessResource(ClaimsPrincipal user, string resourceUserId);

    /// <summary>
    /// Validate that the user is authenticated and return the user ID
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the controller</param>
    /// <returns>ApiResponse with user ID if valid, error if not</returns>
    ApiResponse<string> ValidateAuthentication(ClaimsPrincipal user);

    /// <summary>
    /// Get user information from the database by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Check if a user exists and is active
    /// </summary>
    /// <param name="userId">User ID to check</param>
    /// <returns>True if user exists and is active</returns>
    Task<bool> IsUserActiveAsync(string userId);

    /// <summary>
    /// Get user roles from the database
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of user roles</returns>
    Task<List<string>> GetUserRolesAsync(string userId);
}

public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly UserManager<User> _userManager;

    public AuthenticationHelper(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public string? GetCurrentUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public List<string> GetCurrentUserRoles(ClaimsPrincipal user)
    {
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    public bool HasRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    public bool IsAdmin(ClaimsPrincipal user)
    {
        return HasRole(user, "Admin");
    }

    public bool CanAccessResource(ClaimsPrincipal user, string resourceUserId)
    {
        var currentUserId = GetCurrentUserId(user);
        return currentUserId == resourceUserId || IsAdmin(user);
    }

    public ApiResponse<string> ValidateAuthentication(ClaimsPrincipal user)
    {
        var userId = GetCurrentUserId(user);
        
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "User is not authenticated",
                Errors = new List<string> { "Authentication required" }
            };
        }

        return new ApiResponse<string>
        {
            Success = true,
            Message = "User authenticated",
            Data = userId
        };
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<bool> IsUserActiveAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return user != null && user.IsActive;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }
}
