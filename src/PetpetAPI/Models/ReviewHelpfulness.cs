using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.Models;

public class ReviewHelpfulness
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty; // User who voted
    
    [Required]
    public int ReviewId { get; set; } // Review being voted on
    
    public bool IsHelpful { get; set; } // true = helpful, false = unhelpful
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Review Review { get; set; } = null!;
}
