using System.ComponentModel.DataAnnotations;

namespace PetpetAPI.DTOs.Cart;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => ProductPrice * Quantity;
    public bool IsInStock { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddToCartDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

public class CartSummaryDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalItems => Items.Sum(x => x.Quantity);
    public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
    public int UniqueProductCount => Items.Count;
    public bool HasOutOfStockItems => Items.Any(x => !x.IsInStock || x.Quantity > x.StockQuantity);
}
