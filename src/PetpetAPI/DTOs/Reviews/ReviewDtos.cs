using System.ComponentModel.DataAnnotations;
using PetpetAPI.Models;

namespace PetpetAPI.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerProfilePicture { get; set; } = string.Empty;
    public string? ReviewedUserId { get; set; }
    public string? ReviewedUserName { get; set; }
    public string? ServiceName { get; set; }
    public ReviewType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsApproved { get; set; }
}

public class CreateReviewDto
{
    public string? ReviewedUserId { get; set; }
    
    public string? ServiceName { get; set; }
    
    [Required]
    public ReviewType Type { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 1000 characters")]
    public string Comment { get; set; } = string.Empty;
}

public class ReviewFilterDto
{
    public string? ReviewedUserId { get; set; }
    public string? ReviewerId { get; set; }
    public string? ServiceName { get; set; }
    public ReviewType? Type { get; set; }
    public int? Rating { get; set; }
    public bool? IsApproved { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserReviewSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new();
}

public class ServiceReviewSummaryDto
{
    public string ServiceName { get; set; } = string.Empty;
    public ReviewType Type { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new();
}
