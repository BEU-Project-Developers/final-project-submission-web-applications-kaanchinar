using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs.Admin;
using PetpetAPI.DTOs.Common;
using PetpetAPI.DTOs.Products;
using PetpetAPI.Models;

namespace PetpetAPI.Services;

public interface IAdminService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    Task<ApiResponse<List<LowStockProductDto>>> GetLowStockProductsAsync();
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        // Get total products count
        var totalProducts = await _context.Products
            .Where(p => p.IsActive)
            .CountAsync();

        // Calculate inventory value
        var inventoryValue = await _context.Products
            .Where(p => p.IsActive)
            .SumAsync(p => p.Price * p.StockQuantity);

        // Get low stock products count
        var lowStockProductsCount = await _context.Products
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
            .CountAsync();

        // Get total categories count
        var totalCategories = Enum.GetValues(typeof(ProductCategory)).Length;

        // Get recent 6 products
        var recentProducts = await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Brand = p.Brand,
                StockQuantity = p.StockQuantity,
                Section = p.Section,
                Category = p.Category,
                State = p.State,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).OrderBy(i => i.DisplayOrder).ToList()
            })
            .ToListAsync();

        var dashboardStats = new DashboardStatsDto
        {
            TotalProducts = totalProducts,
            InventoryValue = inventoryValue,
            LowStockProductsCount = lowStockProductsCount,
            TotalCategories = totalCategories,
            RecentProducts = recentProducts
        };

        return new ApiResponse<DashboardStatsDto>
        {
            Success = true,
            Message = "Dashboard statistics retrieved successfully",
            Data = dashboardStats
        };
    }

    public async Task<ApiResponse<List<LowStockProductDto>>> GetLowStockProductsAsync()
    {
        var lowStockProducts = await _context.Products
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
            .Select(p => new LowStockProductDto
            {
                Id = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                Brand = p.Brand,
                Price = p.Price
            })
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        return new ApiResponse<List<LowStockProductDto>>
        {
            Success = true,
            Message = "Low stock products retrieved successfully",
            Data = lowStockProducts
        };
    }
}
