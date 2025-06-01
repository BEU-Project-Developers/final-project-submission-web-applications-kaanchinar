using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.Models;

public class Pet
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Species { get; set; } = string.Empty; // Dog, Cat, Bird, etc.
    
    [StringLength(100)]
    public string? Breed { get; set; }
    
    public int? Age { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Owner relationship
    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = null!;
    
    // Reviews relationship
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
