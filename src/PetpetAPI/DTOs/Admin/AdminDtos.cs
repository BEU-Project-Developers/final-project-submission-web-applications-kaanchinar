using PetpetAPI.DTOs.Products;

namespace PetpetAPI.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalProducts { get; set; }
    public decimal InventoryValue { get; set; }
    public int LowStockProductsCount { get; set; }
    public int TotalCategories { get; set; }
    public List<ProductDto> RecentProducts { get; set; } = new();
}

public class LowStockProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
