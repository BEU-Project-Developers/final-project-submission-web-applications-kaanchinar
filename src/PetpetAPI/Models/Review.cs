using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.Models;

public class Review
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty; // User who wrote the review
    
    [Required]
    public int ProductId { get; set; } // Product being reviewed
    
    [Required]
    public int OrderId { get; set; } // Order from which the product was purchased
    
    [Range(1, 5)]
    public int Rating { get; set; } // 1-5 stars
    
    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Title { get; set; } = string.Empty; // Review title/headline
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsApproved { get; set; } = true; // For moderation if needed
    public bool IsVerifiedPurchase { get; set; } = false; // Verified purchase badge
    
    // Helpful votes
    public int HelpfulVotes { get; set; } = 0;
    public int UnhelpfulVotes { get; set; } = 0;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual ICollection<ReviewHelpfulness> ReviewHelpfulness { get; set; } = new List<ReviewHelpfulness>();
}
