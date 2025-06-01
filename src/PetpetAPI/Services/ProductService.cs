using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs.Common;
using PetpetAPI.DTOs.Products;
using PetpetAPI.Models;

namespace PetpetAPI.Services;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductFilterDto filter);
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<ApiResponse<bool>> DeleteProductAsync(int id);
    Task<ApiResponse<List<ProductDto>>> GetLowStockProductsAsync();
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        // Apply filters
        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.Section.HasValue)
            query = query.Where(p => p.Section == filter.Section.Value);

        if (filter.Category.HasValue)
            query = query.Where(p => p.Category == filter.Category.Value);

        if (!string.IsNullOrEmpty(filter.Brand))
            query = query.Where(p => p.Brand.ToLower().Contains(filter.Brand.ToLower()));

        if (filter.State.HasValue)
            query = query.Where(p => p.State == filter.State.Value);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.Brand.ToLower().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Brand = p.Brand,
                StockQuantity = p.StockQuantity,
                Section = p.Section,
                Category = p.Category,
                State = p.State,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = 0,
                AverageRating = 0,
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).OrderBy(i => i.DisplayOrder).ToList()
            })
            .ToListAsync();

        var result = new PagedResult<ProductDto>
        {
            Items = products,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };

        return new ApiResponse<PagedResult<ProductDto>>
        {
            Success = true,
            Data = result,
            Message = "Products retrieved successfully"
        };
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Brand = p.Brand,
                StockQuantity = p.StockQuantity,
                Section = p.Section,
                Category = p.Category,
                State = p.State,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = 0,
                AverageRating = 0,
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).OrderBy(i => i.DisplayOrder).ToList()
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Data = product,
            Message = "Product retrieved successfully"
        };
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            OriginalPrice = createProductDto.OriginalPrice,
            Brand = createProductDto.Brand,
            StockQuantity = createProductDto.StockQuantity,
            LowStockThreshold = createProductDto.LowStockThreshold,
            Section = createProductDto.Section,
            Category = createProductDto.Category,
            State = createProductDto.State
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Add product images
        var productImages = new List<ProductImage>();
        for (int i = 0; i < createProductDto.Images.Count; i++)
        {
            var imageDto = createProductDto.Images[i];
            var productImage = new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imageDto.ImageUrl,
                AltText = !string.IsNullOrEmpty(imageDto.AltText) ? imageDto.AltText : product.Name,
                DisplayOrder = imageDto.DisplayOrder > 0 ? imageDto.DisplayOrder : i + 1,
                IsPrimary = imageDto.IsPrimary || i == 0 // First image is primary if none specified
            };
            productImages.Add(productImage);
        }

        if (productImages.Any())
        {
            _context.ProductImages.AddRange(productImages);
            await _context.SaveChangesAsync();
        }

        // Load the product with images for the response
        var productWithImages = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Brand = product.Brand,
            StockQuantity = product.StockQuantity,
            Section = product.Section,
            Category = product.Category,
            State = product.State,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            ReviewCount = 0,
            AverageRating = 0,
            Images = productWithImages?.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).OrderBy(i => i.DisplayOrder).ToList() ?? new List<ProductImageDto>()
        };

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Data = productDto,
            Message = "Product created successfully"
        };
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null)
        {
            return new ApiResponse<ProductDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.OriginalPrice = updateProductDto.OriginalPrice;
        product.Brand = updateProductDto.Brand;
        product.StockQuantity = updateProductDto.StockQuantity;
        product.LowStockThreshold = updateProductDto.LowStockThreshold;
        product.Section = updateProductDto.Section;
        product.Category = updateProductDto.Category;
        product.State = updateProductDto.State;
        product.UpdatedAt = DateTime.UtcNow;

        // Update images: Remove existing and add new ones
        if (updateProductDto.Images.Any())
        {
            // Remove existing images
            _context.ProductImages.RemoveRange(product.Images);
            
            // Add new images
            var productImages = new List<ProductImage>();
            for (int i = 0; i < updateProductDto.Images.Count; i++)
            {
                var imageDto = updateProductDto.Images[i];
                var productImage = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = imageDto.ImageUrl,
                    AltText = !string.IsNullOrEmpty(imageDto.AltText) ? imageDto.AltText : product.Name,
                    DisplayOrder = imageDto.DisplayOrder > 0 ? imageDto.DisplayOrder : i + 1,
                    IsPrimary = imageDto.IsPrimary || i == 0 // First image is primary if none specified
                };
                productImages.Add(productImage);
            }
            
            _context.ProductImages.AddRange(productImages);
        }

        await _context.SaveChangesAsync();

        // Reload the product with updated images
        var updatedProduct = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Brand = product.Brand,
            StockQuantity = product.StockQuantity,
            Section = product.Section,
            Category = product.Category,
            State = product.State,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            ReviewCount = 0,
            AverageRating = 0,
            Images = updatedProduct?.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                DisplayOrder = i.DisplayOrder,
                IsPrimary = i.IsPrimary
            }).OrderBy(i => i.DisplayOrder).ToList() ?? new List<ProductImageDto>()
        };

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Data = productDto,
            Message = "Product updated successfully"
        };
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Product deleted successfully"
        };
    }

    public async Task<ApiResponse<List<ProductDto>>> GetLowStockProductsAsync()
    {
        var products = await _context.Products
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.StockQuantity <= p.LowStockThreshold)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                Brand = p.Brand,
                StockQuantity = p.StockQuantity,
                Section = p.Section,
                Category = p.Category,
                State = p.State,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ReviewCount = 0,
                AverageRating = 0,
                Images = p.Images.Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).OrderBy(i => i.DisplayOrder).ToList()
            })
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        return new ApiResponse<List<ProductDto>>
        {
            Success = true,
            Data = products,
            Message = "Low stock products retrieved successfully"
        };
    }
}
