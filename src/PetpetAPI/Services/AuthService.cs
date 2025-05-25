using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs.Auth;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Models;
using System.Security.Claims;

namespace PetpetAPI.Services;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<ApiResponse<bool>> LogoutAsync(string userId);
    Task<ApiResponse<bool>> RevokeRefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "User with this email already exists",
                Errors = new List<string> { "Email is already registered" }
            };
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Failed to create user",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            }
        };

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "User registered successfully",
            Data = authResponse
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid email or password",
                Errors = new List<string> { "Authentication failed" }
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid email or password",
                Errors = new List<string> { "Authentication failed" }
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        var authResponse = new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            }
        };

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Login successful",
            Data = authResponse
        };
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken && !rt.IsRevoked);

        if (refreshToken == null || refreshToken.ExpiryDate <= DateTime.UtcNow)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "Invalid or expired refresh token",
                Errors = new List<string> { "Token validation failed" }
            };
        }

        var user = refreshToken.User;
        if (!user.IsActive)
        {
            return new ApiResponse<AuthResponseDto>
            {
                Success = false,
                Message = "User account is inactive",
                Errors = new List<string> { "Account access denied" }
            };
        }

        // Revoke the old refresh token
        refreshToken.IsRevoked = true;
        
        // Create new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id);

        await _context.SaveChangesAsync();

        var authResponse = new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60")),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            }
        };

        return new ApiResponse<AuthResponseDto>
        {
            Success = true,
            Message = "Token refreshed successfully",
            Data = authResponse
        };
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Logged out successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Refresh token not found",
                Data = false
            };
        }

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Refresh token revoked successfully",
            Data = true
        };
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = _jwtService.GenerateRefreshToken(),
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
