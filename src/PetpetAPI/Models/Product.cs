namespace PetpetAPI.Models;

public enum AnimalSection
{
    Cats = 1,
    Dogs = 2,
    Other = 3
}

public enum ProductCategory
{
    Toys = 1,
    Food = 2,
    Litters = 3,
    Medicines = 4,
    Accessories = 5,
    Grooming = 6
}

public enum ProductState
{
    OutOfStock = 0,
    InStock = 1,
    NewProduct = 2
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public AnimalSection Section { get; set; }
    public ProductCategory Category { get; set; }
    public ProductState State { get; set; } = ProductState.InStock;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
