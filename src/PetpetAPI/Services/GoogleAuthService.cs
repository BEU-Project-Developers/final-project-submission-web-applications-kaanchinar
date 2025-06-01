using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using PetpetAPI.DTOs.Auth;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Models;
using System.Security.Claims;

namespace PetpetAPI.Services;

public interface IGoogleAuthService
{
    Task<ApiResponse<AuthResponseDto>> AuthenticateWithGoogleAsync(string idToken);
    Task<ApiResponse<string>> GetGoogleAuthUrlAsync(string state);
    Task<ApiResponse<AuthResponseDto>> HandleGoogleCallbackAsync(string code);
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;

    public GoogleAuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        IConfiguration configuration,
        IAuthService authService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _authService = authService;
    }

    public async Task<ApiResponse<AuthResponseDto>> AuthenticateWithGoogleAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { _configuration["GoogleAuth:ClientId"] ?? "" }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Invalid Google token"
                };
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    AuthProvider = "Google",
                    ExternalId = payload.Subject
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Failed to create user account",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Add default role
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                // Update user information if needed
                var needsUpdate = false;
                
                if (string.IsNullOrEmpty(user.ExternalId))
                {
                    user.ExternalId = payload.Subject;
                    user.AuthProvider = "Google";
                    needsUpdate = true;
                }

                if (user.FirstName != payload.GivenName && !string.IsNullOrEmpty(payload.GivenName))
                {
                    user.FirstName = payload.GivenName;
                    needsUpdate = true;
                }

                if (user.LastName != payload.FamilyName && !string.IsNullOrEmpty(payload.FamilyName))
                {
                    user.LastName = payload.FamilyName;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await _userManager.UpdateAsync(user);
                }
            }

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Google authentication successful",
                Data = new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = $"Google authentication failed: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<string>> GetGoogleAuthUrlAsync(string state)
    {
        await Task.CompletedTask; // Placeholder for async context

        var clientId = _configuration["GoogleAuth:ClientId"];
        var redirectUri = $"{_configuration["ApiBaseUrl"]}/api/auth/google/callback";
        
        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                     $"client_id={clientId}&" +
                     $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                     $"response_type=code&" +
                     $"scope={Uri.EscapeDataString("openid email profile")}&" +
                     $"state={state}";

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Google auth URL generated",
            Data = authUrl
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> HandleGoogleCallbackAsync(string code)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            var clientSecret = _configuration["GoogleAuth:ClientSecret"];
            var redirectUri = $"{_configuration["ApiBaseUrl"]}/api/auth/google/callback";

            // Exchange code for tokens
            using var httpClient = new HttpClient();
            var tokenRequest = new Dictionary<string, string>
            {
                ["client_id"] = clientId ?? "",
                ["client_secret"] = clientSecret ?? "",
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = redirectUri
            };

            var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Failed to exchange code for token"
                };
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(tokenJson);

            if (tokenData == null || !tokenData.ContainsKey("id_token"))
            {
                return new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "No ID token received from Google"
                };
            }

            var idToken = tokenData["id_token"].ToString();
            return await AuthenticateWithGoogleAsync(idToken!);
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = $"Google callback handling failed: {ex.Message}"
            };
        }
    }
}
