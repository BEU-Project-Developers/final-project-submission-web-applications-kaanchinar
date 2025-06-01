using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.DTOs;

public class CreateReviewDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
}

public class UpdateReviewDto
{
    [Range(1, 5)]
    public int? Rating { get; set; }
    
    [StringLength(1000)]
    public string? Comment { get; set; }
    
    [StringLength(100)]
    public string? Title { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulVotes { get; set; }
    public int UnhelpfulVotes { get; set; }
    public bool? UserHelpfulnessVote { get; set; } // null = no vote, true = helpful, false = unhelpful
}

public class ProductReviewSummaryDto
{
    public int ProductId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Rating -> Count
}

public class ReviewHelpfulnessDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ReviewId must be a positive number")]
    public int ReviewId { get; set; }
    
    [Required]
    public bool IsHelpful { get; set; }
}
