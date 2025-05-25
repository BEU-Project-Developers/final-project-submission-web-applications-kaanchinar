using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs.Cart;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Models;

namespace PetpetAPI.Services;

public interface ICartService
{
    Task<ApiResponse<CartSummaryDto>> GetCartAsync(string userId);
    Task<ApiResponse<CartItemDto>> AddToCartAsync(string userId, AddToCartDto addToCartDto);
    Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto updateCartItemDto);
    Task<ApiResponse<bool>> RemoveFromCartAsync(string userId, int cartItemId);
    Task<ApiResponse<bool>> ClearCartAsync(string userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CartSummaryDto>> GetCartAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Where(ci => ci.UserId == userId)
            .Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                ProductImageUrl = ci.Product.Images.FirstOrDefault(i => i.IsPrimary) != null 
                    ? ci.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl 
                    : ci.Product.Images.FirstOrDefault() != null 
                        ? ci.Product.Images.FirstOrDefault()!.ImageUrl 
                        : "",
                Quantity = ci.Quantity,
                IsInStock = ci.Product.IsActive && ci.Product.StockQuantity > 0,
                StockQuantity = ci.Product.StockQuantity,
                CreatedAt = ci.CreatedAt
            })
            .ToListAsync();

        var cartSummary = new CartSummaryDto
        {
            Items = cartItems
        };

        return new ApiResponse<CartSummaryDto>
        {
            Success = true,
            Message = "Cart retrieved successfully",
            Data = cartSummary
        };
    }

    public async Task<ApiResponse<CartItemDto>> AddToCartAsync(string userId, AddToCartDto addToCartDto)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == addToCartDto.ProductId && p.IsActive);

        if (product == null)
        {
            return new ApiResponse<CartItemDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (product.StockQuantity < addToCartDto.Quantity)
        {
            return new ApiResponse<CartItemDto>
            {
                Success = false,
                Message = "Insufficient stock"
            };
        }

        // Check if item already exists in cart
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == addToCartDto.ProductId);

        if (existingCartItem != null)
        {
            // Update quantity
            existingCartItem.Quantity += addToCartDto.Quantity;
            existingCartItem.UpdatedAt = DateTime.UtcNow;

            if (product.StockQuantity < existingCartItem.Quantity)
            {
                return new ApiResponse<CartItemDto>
                {
                    Success = false,
                    Message = "Insufficient stock"
                };
            }
        }
        else
        {
            // Create new cart item
            existingCartItem = new CartItem
            {
                UserId = userId,
                ProductId = addToCartDto.ProductId,
                Quantity = addToCartDto.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(existingCartItem);
        }

        await _context.SaveChangesAsync();

        var cartItemDto = new CartItemDto
        {
            Id = existingCartItem.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductPrice = product.Price,
            ProductImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? 
                      product.Images.FirstOrDefault()?.ImageUrl ?? "",
            Quantity = existingCartItem.Quantity,
            IsInStock = product.IsActive && product.StockQuantity > 0,
            StockQuantity = product.StockQuantity,
            CreatedAt = existingCartItem.CreatedAt
        };

        return new ApiResponse<CartItemDto>
        {
            Success = true,
            Message = "Product added to cart successfully",
            Data = cartItemDto
        };
    }

    public async Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemDto updateCartItemDto)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

        if (cartItem == null)
        {
            return new ApiResponse<CartItemDto>
            {
                Success = false,
                Message = "Cart item not found"
            };
        }

        if (cartItem.Product.StockQuantity < updateCartItemDto.Quantity)
        {
            return new ApiResponse<CartItemDto>
            {
                Success = false,
                Message = "Insufficient stock"
            };
        }

        cartItem.Quantity = updateCartItemDto.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var cartItemDto = new CartItemDto
        {
            Id = cartItem.Id,
            ProductId = cartItem.Product.Id,
            ProductName = cartItem.Product.Name,
            ProductPrice = cartItem.Product.Price,
            ProductImageUrl = cartItem.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? 
                      cartItem.Product.Images.FirstOrDefault()?.ImageUrl ?? "",
            Quantity = cartItem.Quantity,
            IsInStock = cartItem.Product.IsActive && cartItem.Product.StockQuantity > 0,
            StockQuantity = cartItem.Product.StockQuantity,
            CreatedAt = cartItem.CreatedAt
        };

        return new ApiResponse<CartItemDto>
        {
            Success = true,
            Message = "Cart item updated successfully",
            Data = cartItemDto
        };
    }

    public async Task<ApiResponse<bool>> RemoveFromCartAsync(string userId, int cartItemId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == userId);

        if (cartItem == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Cart item not found"
            };
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Item removed from cart successfully",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (cartItems.Any())
        {
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Cart cleared successfully",
            Data = true
        };
    }
}
