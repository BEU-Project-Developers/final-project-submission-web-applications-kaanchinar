using PetpetAPI.Models;
using System.Text.Json.Serialization;

namespace PetpetAPI.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public AnimalSection Section { get; set; }
    public ProductCategory Category { get; set; }
    public ProductState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    
    // Review summary
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateProductDto
{
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
    [JsonPropertyName("images")]
    public List<CreateProductImageDto> Images { get; set; } = new();
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public AnimalSection Section { get; set; }
    public ProductCategory Category { get; set; }
    public ProductState State { get; set; }
    [JsonPropertyName("images")]
    public List<CreateProductImageDto> Images { get; set; } = new();
}

public class CreateProductImageDto
{
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
    [JsonPropertyName("altText")]
    public string AltText { get; set; } = string.Empty;
    [JsonPropertyName("displayOrder")]
    public int DisplayOrder { get; set; } = 1;
    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; } = false;
}

public class ProductFilterDto
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public AnimalSection? Section { get; set; }
    public ProductCategory? Category { get; set; }
    public string? Brand { get; set; }
    public ProductState? State { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}
