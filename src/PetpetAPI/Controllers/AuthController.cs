using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Auth;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Services;
using System.Security.Claims;

namespace PetpetAPI.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(IAuthService authService, IGoogleAuthService googleAuthService)
    {
        _authService = authService;
        _googleAuthService = googleAuthService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="registerDto">User registration information</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        
        if (!result.Success)
            return BadRequest(result);

        // Set JWT token in HTTP-only cookie
        if (result.Data != null)
            SetAuthCookies(result.Data);

        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="loginDto">User login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        if (!result.Success)
            return BadRequest(result);

        // Set JWT token in HTTP-only cookie
        if (result.Data != null)
            SetAuthCookies(result.Data);

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token</param>
    /// <returns>New authentication response with fresh tokens</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Logout and revoke all refresh tokens for the current user
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.LogoutAsync(userId);

        // Clear authentication cookies
        ClearAuthCookies();

        return Ok(result);
    }

    /// <summary>
    /// Revoke a specific refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token to revoke</param>
    /// <returns>Revocation confirmation</returns>
    [HttpPost("revoke")]
    public async Task<ActionResult<ApiResponse<bool>>> RevokeRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
        return Ok(result);
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.GetCurrentUserAsync(userId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Sets JWT and refresh tokens in HTTP-only cookies
    /// </summary>
    /// <param name="authResponse">Authentication response containing tokens</param>
    private void SetAuthCookies(AuthResponseDto authResponse)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Requires HTTPS in production
            SameSite = SameSiteMode.Lax,
            Expires = authResponse.ExpiresAt
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Requires HTTPS in production
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7) // Refresh token typically lasts longer
        };

        Response.Cookies.Append("authToken", authResponse.Token, cookieOptions);
        Response.Cookies.Append("refreshToken", authResponse.RefreshToken, refreshCookieOptions);
    }

    /// <summary>
    /// Clears authentication cookies
    /// </summary>
    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("authToken");
        Response.Cookies.Delete("refreshToken");
    }

    /// <summary>
    /// Authenticate with Google ID token
    /// </summary>
    /// <param name="googleTokenDto">Google ID token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("google")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleAuth([FromBody] GoogleTokenDto googleTokenDto)
    {
        var result = await _googleAuthService.AuthenticateWithGoogleAsync(googleTokenDto.IdToken);
        
        if (!result.Success)
            return BadRequest(result);

        // Set JWT token in HTTP-only cookie
        if (result.Data != null)
            SetAuthCookies(result.Data);

        return Ok(result);
    }

    /// <summary>
    /// Get Google OAuth authorization URL
    /// </summary>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <returns>Google OAuth authorization URL</returns>
    [HttpGet("google/url")]
    public async Task<ActionResult<ApiResponse<string>>> GetGoogleAuthUrl([FromQuery] string state = "")
    {
        if (string.IsNullOrEmpty(state))
            state = Guid.NewGuid().ToString();

        var result = await _googleAuthService.GetGoogleAuthUrlAsync(state);
        return Ok(result);
    }

    /// <summary>
    /// Handle Google OAuth callback
    /// </summary>
    /// <param name="code">Authorization code from Google</param>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <returns>Redirect to frontend with authentication result</returns>
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
            return Redirect($"{frontendUrl}/auth/error?message=Authorization code not provided");
        }

        var result = await _googleAuthService.HandleGoogleCallbackAsync(code);
        
        var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
        
        if (result.Success && result.Data != null)
        {
            // Set JWT token in HTTP-only cookie
            SetAuthCookies(result.Data);
            
            // Redirect to frontend success page
            return Redirect($"{frontendBaseUrl}/auth/success");
        }
        else
        {
            // Redirect to frontend error page
            var errorMessage = Uri.EscapeDataString(result.Message);
            return Redirect($"{frontendBaseUrl}/auth/error?message={errorMessage}");
        }
    }
}
