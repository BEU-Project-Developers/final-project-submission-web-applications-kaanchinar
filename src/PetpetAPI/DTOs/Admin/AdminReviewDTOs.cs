using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.DTOs.Admin;

public class AdminReviewDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public int HelpfulVotes { get; set; }
    public int UnhelpfulVotes { get; set; }
    public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
}

public class AdminReviewFilterDto
{
    public string? SearchTerm { get; set; }
    public int? ProductId { get; set; }
    public string? UserId { get; set; }
    public bool? IsApproved { get; set; }
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "CreatedAt"; // CreatedAt, Rating, HelpfulVotes
    public string SortDirection { get; set; } = "desc"; // asc, desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminReviewActionDto
{
    [Required]
    public int ReviewId { get; set; }
    
    [Required]
    public string Action { get; set; } = string.Empty; // "approve", "reject", "delete"
    
    public string? Reason { get; set; }
}

public class BulkReviewActionDto
{
    [Required]
    public List<int> ReviewIds { get; set; } = new();
    
    [Required]
    public string Action { get; set; } = string.Empty; // "approve", "reject", "delete"
    
    public string? Reason { get; set; }
}

public class ReviewStatsDto
{
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int RejectedReviews { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsToday { get; set; }
    public int ReviewsThisWeek { get; set; }
    public int ReviewsThisMonth { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
}

public class PagedReviewsDto
{
    public List<AdminReviewDto> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
