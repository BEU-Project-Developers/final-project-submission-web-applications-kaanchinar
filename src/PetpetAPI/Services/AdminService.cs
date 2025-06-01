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
    
    // Review Management
    Task<ApiResponse<PagedReviewsDto>> GetReviewsAsync(AdminReviewFilterDto filter);
    Task<ApiResponse<ReviewStatsDto>> GetReviewStatsAsync();
    Task<ApiResponse<AdminReviewDto>> GetReviewDetailsAsync(int reviewId);
    Task<ApiResponse<bool>> ModerateReviewAsync(AdminReviewActionDto action);
    Task<ApiResponse<bool>> BulkModerateReviewsAsync(BulkReviewActionDto action);
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
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

    // Review Management Methods
    public async Task<ApiResponse<PagedReviewsDto>> GetReviewsAsync(AdminReviewFilterDto filter)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(r => 
                r.Comment.Contains(filter.SearchTerm) ||
                r.Title.Contains(filter.SearchTerm) ||
                r.User.FirstName.Contains(filter.SearchTerm) ||
                r.User.LastName.Contains(filter.SearchTerm) ||
                r.Product.Name.Contains(filter.SearchTerm));
        }

        if (filter.ProductId.HasValue)
        {
            query = query.Where(r => r.ProductId == filter.ProductId.Value);
        }

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            query = query.Where(r => r.UserId == filter.UserId);
        }

        if (filter.IsApproved.HasValue)
        {
            query = query.Where(r => r.IsApproved == filter.IsApproved.Value);
        }

        if (filter.MinRating.HasValue)
        {
            query = query.Where(r => r.Rating >= filter.MinRating.Value);
        }

        if (filter.MaxRating.HasValue)
        {
            query = query.Where(r => r.Rating <= filter.MaxRating.Value);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);
        }

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "rating" => filter.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(r => r.Rating) 
                : query.OrderByDescending(r => r.Rating),
            "helpfulvotes" => filter.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(r => r.HelpfulVotes) 
                : query.OrderByDescending(r => r.HelpfulVotes),
            _ => filter.SortDirection.ToLower() == "asc" 
                ? query.OrderBy(r => r.CreatedAt) 
                : query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var reviews = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new AdminReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.FirstName + " " + r.User.LastName,
                UserEmail = r.User.Email ?? "",
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                OrderId = r.OrderId,
                Rating = r.Rating,
                Comment = r.Comment,
                Title = r.Title,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                IsApproved = r.IsApproved,
                HelpfulVotes = r.HelpfulVotes,
                UnhelpfulVotes = r.UnhelpfulVotes,
                Status = r.IsApproved ? "Approved" : "Pending"
            })
            .ToListAsync();

        var result = new PagedReviewsDto
        {
            Reviews = reviews,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };

        return new ApiResponse<PagedReviewsDto>
        {
            Success = true,
            Message = "Reviews retrieved successfully",
            Data = result
        };
    }

    public async Task<ApiResponse<ReviewStatsDto>> GetReviewStatsAsync()
    {
        var totalReviews = await _context.Reviews.CountAsync();
        var pendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);
        var approvedReviews = await _context.Reviews.CountAsync(r => r.IsApproved);
        var rejectedReviews = 0; // Assuming rejected reviews are deleted or marked differently

        var averageRating = totalReviews > 0 
            ? await _context.Reviews.AverageAsync(r => r.Rating) 
            : 0;

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var reviewsToday = await _context.Reviews.CountAsync(r => r.CreatedAt.Date == today);
        var reviewsThisWeek = await _context.Reviews.CountAsync(r => r.CreatedAt >= weekStart);
        var reviewsThisMonth = await _context.Reviews.CountAsync(r => r.CreatedAt >= monthStart);

        var ratingDistribution = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            ratingDistribution[i] = await _context.Reviews.CountAsync(r => r.Rating == i);
        }

        var stats = new ReviewStatsDto
        {
            TotalReviews = totalReviews,
            PendingReviews = pendingReviews,
            ApprovedReviews = approvedReviews,
            RejectedReviews = rejectedReviews,
            AverageRating = Math.Round(averageRating, 2),
            ReviewsToday = reviewsToday,
            ReviewsThisWeek = reviewsThisWeek,
            ReviewsThisMonth = reviewsThisMonth,
            RatingDistribution = ratingDistribution
        };

        return new ApiResponse<ReviewStatsDto>
        {
            Success = true,
            Message = "Review statistics retrieved successfully",
            Data = stats
        };
    }

    public async Task<ApiResponse<AdminReviewDto>> GetReviewDetailsAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            return new ApiResponse<AdminReviewDto>
            {
                Success = false,
                Message = "Review not found"
            };
        }

        var reviewDto = new AdminReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User.FirstName + " " + review.User.LastName,
            UserEmail = review.User.Email ?? "",
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Comment = review.Comment,
            Title = review.Title,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            IsApproved = review.IsApproved,
            HelpfulVotes = review.HelpfulVotes,
            UnhelpfulVotes = review.UnhelpfulVotes,
            Status = review.IsApproved ? "Approved" : "Pending"
        };

        return new ApiResponse<AdminReviewDto>
        {
            Success = true,
            Message = "Review details retrieved successfully",
            Data = reviewDto
        };
    }

    public async Task<ApiResponse<bool>> ModerateReviewAsync(AdminReviewActionDto action)
    {
        _logger.LogInformation("Attempting to {Action} review with ID {ReviewId}", action.Action, action.ReviewId);
        Console.WriteLine($"[DEBUG] AdminService.ModerateReviewAsync called: Action={action.Action}, ReviewId={action.ReviewId}");
        
        var review = await _context.Reviews
            .Include(r => r.ReviewHelpfulness) // Include related helpfulness votes
            .FirstOrDefaultAsync(r => r.Id == action.ReviewId);
            
        if (review == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} not found", action.ReviewId);
            Console.WriteLine($"[DEBUG] Review with ID {action.ReviewId} not found in database");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Review not found"
            };
        }

        _logger.LogInformation("Found review with ID {ReviewId}, has {HelpfulnessCount} helpfulness votes", 
            action.ReviewId, review.ReviewHelpfulness.Count);
        Console.WriteLine($"[DEBUG] Found review: ID={review.Id}, UserId={review.UserId}, HelpfulnessVotes={review.ReviewHelpfulness.Count}");

        switch (action.Action.ToLower())
        {
            case "approve":
                review.IsApproved = true;
                review.UpdatedAt = DateTime.UtcNow;
                break;
            case "reject":
                review.IsApproved = false;
                review.UpdatedAt = DateTime.UtcNow;
                break;
            case "delete":
                // First remove all related helpfulness votes
                if (review.ReviewHelpfulness.Any())
                {
                    _logger.LogInformation("Removing {Count} helpfulness votes for review {ReviewId}", 
                        review.ReviewHelpfulness.Count, action.ReviewId);
                    _context.ReviewHelpfulness.RemoveRange(review.ReviewHelpfulness);
                }
                // Then remove the review
                _logger.LogInformation("Removing review with ID {ReviewId}", action.ReviewId);
                _context.Reviews.Remove(review);
                break;
            default:
                _logger.LogWarning("Invalid action '{Action}' for review {ReviewId}", action.Action, action.ReviewId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid action. Use 'approve', 'reject', or 'delete'"
                };
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully {Action}d review with ID {ReviewId}", action.Action, action.ReviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to {Action} review with ID {ReviewId}", action.Action, action.ReviewId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to {action.Action} review: {ex.Message}"
            };
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = $"Review {action.Action}d successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> BulkModerateReviewsAsync(BulkReviewActionDto action)
    {
        var reviews = await _context.Reviews
            .Include(r => r.ReviewHelpfulness) // Include related helpfulness votes
            .Where(r => action.ReviewIds.Contains(r.Id))
            .ToListAsync();

        if (!reviews.Any())
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "No reviews found for the provided IDs"
            };
        }

        switch (action.Action.ToLower())
        {
            case "approve":
                foreach (var review in reviews)
                {
                    review.IsApproved = true;
                    review.UpdatedAt = DateTime.UtcNow;
                }
                break;
            case "reject":
                foreach (var review in reviews)
                {
                    review.IsApproved = false;
                    review.UpdatedAt = DateTime.UtcNow;
                }
                break;
            case "delete":
                // First remove all related helpfulness votes
                var helpfulnessVotes = reviews.SelectMany(r => r.ReviewHelpfulness).ToList();
                if (helpfulnessVotes.Any())
                {
                    _context.ReviewHelpfulness.RemoveRange(helpfulnessVotes);
                }
                // Then remove the reviews
                _context.Reviews.RemoveRange(reviews);
                break;
            default:
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid action. Use 'approve', 'reject', or 'delete'"
                };
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to {action.Action} reviews: {ex.Message}"
            };
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = $"{reviews.Count} review(s) {action.Action}d successfully",
            Data = true
        };
    }
}
