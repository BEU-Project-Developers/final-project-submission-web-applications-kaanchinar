using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs;
using PetpetAPI.Models;

namespace PetpetAPI.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto dto);
    Task<ReviewDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto dto);
    Task<bool> DeleteReviewAsync(string userId, int reviewId);
    Task<ReviewDto?> GetReviewAsync(int reviewId);
    Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, string? currentUserId = null);
    Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, string? currentUserId = null);
    Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId);
    Task<bool> VoteReviewHelpfulnessAsync(string userId, ReviewHelpfulnessDto dto);
    Task<bool> CanUserReviewProductAsync(string userId, int productId, int orderId);
}

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _context;

    public ReviewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> CreateReviewAsync(string userId, CreateReviewDto dto)
    {
        // Verify user can review this product from this order
        var canReview = await CanUserReviewProductAsync(userId, dto.ProductId, dto.OrderId);
        if (!canReview)
        {
            throw new InvalidOperationException("You can only review products you have purchased from completed orders.");
        }

        // Check if user already reviewed this product from this order
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == dto.ProductId && r.OrderId == dto.OrderId);
        
        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this product from this order.");
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = dto.ProductId,
            OrderId = dto.OrderId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            Title = dto.Title,
            IsVerifiedPurchase = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await GetReviewDtoAsync(review.Id);
    }

    public async Task<ReviewDto> UpdateReviewAsync(string userId, int reviewId, UpdateReviewDto dto)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null)
        {
            throw new InvalidOperationException("Review not found or you don't have permission to update it.");
        }

        if (dto.Rating.HasValue)
            review.Rating = dto.Rating.Value;
        
        if (!string.IsNullOrWhiteSpace(dto.Comment))
            review.Comment = dto.Comment;
        
        if (!string.IsNullOrWhiteSpace(dto.Title))
            review.Title = dto.Title;

        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetReviewDtoAsync(review.Id);
    }

    public async Task<bool> DeleteReviewAsync(string userId, int reviewId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);

        if (review == null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ReviewDto?> GetReviewAsync(int reviewId)
    {
        return await GetReviewDtoAsync(reviewId);
    }

    public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, string? currentUserId = null)
    {
        var query = _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt);

        var reviews = await query.ToListAsync();

        var reviewDtos = new List<ReviewDto>();

        foreach (var review in reviews)
        {
            var dto = MapToReviewDto(review);
            
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var helpfulnessVote = await _context.ReviewHelpfulness
                    .FirstOrDefaultAsync(rh => rh.ReviewId == review.Id && rh.UserId == currentUserId);
                
                dto.UserHelpfulnessVote = helpfulnessVote?.IsHelpful;
            }

            reviewDtos.Add(dto);
        }

        return reviewDtos;
    }

    public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(string userId, string? currentUserId = null)
    {
        var query = _context.Reviews
            .Where(r => r.UserId == userId)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt);

        var reviews = await query.ToListAsync();

        var reviewDtos = new List<ReviewDto>();

        foreach (var review in reviews)
        {
            var dto = MapToReviewDto(review);
            
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var helpfulnessVote = await _context.ReviewHelpfulness
                    .FirstOrDefaultAsync(rh => rh.ReviewId == review.Id && rh.UserId == currentUserId);
                
                dto.UserHelpfulnessVote = helpfulnessVote?.IsHelpful;
            }

            reviewDtos.Add(dto);
        }

        return reviewDtos;
    }

    public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .ToListAsync();

        if (!reviews.Any())
        {
            return new ProductReviewSummaryDto
            {
                ProductId = productId,
                AverageRating = 0,
                TotalReviews = 0,
                RatingDistribution = new Dictionary<int, int>()
            };
        }

        var averageRating = reviews.Average(r => r.Rating);
        var totalReviews = reviews.Count;
        
        var ratingDistribution = reviews
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure all ratings 1-5 are present in distribution
        for (int i = 1; i <= 5; i++)
        {
            if (!ratingDistribution.ContainsKey(i))
                ratingDistribution[i] = 0;
        }

        return new ProductReviewSummaryDto
        {
            ProductId = productId,
            AverageRating = Math.Round(averageRating, 2),
            TotalReviews = totalReviews,
            RatingDistribution = ratingDistribution
        };
    }

    public async Task<bool> VoteReviewHelpfulnessAsync(string userId, ReviewHelpfulnessDto dto)
    {
        // Check if review exists
        var review = await _context.Reviews.FindAsync(dto.ReviewId);
        if (review == null)
            return false;

        // Can't vote on your own review
        if (review.UserId == userId)
            return false;

        // Check existing vote
        var existingVote = await _context.ReviewHelpfulness
            .FirstOrDefaultAsync(rh => rh.ReviewId == dto.ReviewId && rh.UserId == userId);

        if (existingVote != null)
        {
            // Update existing vote
            if (existingVote.IsHelpful != dto.IsHelpful)
            {
                // Update vote counts
                if (existingVote.IsHelpful)
                {
                    review.HelpfulVotes--;
                    review.UnhelpfulVotes++;
                }
                else
                {
                    review.UnhelpfulVotes--;
                    review.HelpfulVotes++;
                }

                existingVote.IsHelpful = dto.IsHelpful;
            }
        }
        else
        {
            // Create new vote
            var newVote = new ReviewHelpfulness
            {
                UserId = userId,
                ReviewId = dto.ReviewId,
                IsHelpful = dto.IsHelpful
            };

            _context.ReviewHelpfulness.Add(newVote);

            // Update vote counts
            if (dto.IsHelpful)
                review.HelpfulVotes++;
            else
                review.UnhelpfulVotes++;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanUserReviewProductAsync(string userId, int productId, int orderId)
    {
        // Check if the order belongs to the user and is completed
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == OrderStatus.Completed);

        if (order == null)
            return false;

        // Check if the product is in the order
        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
        return orderItem != null;
    }

    private async Task<ReviewDto> GetReviewDtoAsync(int reviewId)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new InvalidOperationException("Review not found.");

        return MapToReviewDto(review);
    }

    private static ReviewDto MapToReviewDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User.FirstName + " " + review.User.LastName,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            OrderId = review.OrderId,
            Rating = review.Rating,
            Comment = review.Comment,
            Title = review.Title,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            HelpfulVotes = review.HelpfulVotes,
            UnhelpfulVotes = review.UnhelpfulVotes
        };
    }
}
