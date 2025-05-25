using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Cart;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Services;
using System.Security.Claims;

namespace PetpetAPI.Controllers;

/// <summary>
/// Shopping cart management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get current user's shopping cart
    /// </summary>
    /// <returns>Cart summary with items</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _cartService.GetCartAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Add a product to the shopping cart
    /// </summary>
    /// <param name="addToCartDto">Product and quantity to add</param>
    /// <returns>Added cart item details</returns>
    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartItemDto>>> AddToCart([FromBody] AddToCartDto addToCartDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _cartService.AddToCartAsync(userId, addToCartDto);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Update quantity of a cart item
    /// </summary>
    /// <param name="cartItemId">Cart item ID</param>
    /// <param name="updateCartItemDto">New quantity</param>
    /// <returns>Updated cart item details</returns>
    [HttpPut("items/{cartItemId}")]
    public async Task<ActionResult<ApiResponse<CartItemDto>>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateCartItemDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _cartService.UpdateCartItemAsync(userId, cartItemId, updateCartItemDto);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Remove an item from the shopping cart
    /// </summary>
    /// <param name="cartItemId">Cart item ID to remove</param>
    /// <returns>Removal confirmation</returns>
    [HttpDelete("items/{cartItemId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromCart(int cartItemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Clear all items from the shopping cart
    /// </summary>
    /// <returns>Clear confirmation</returns>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>> ClearCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _cartService.ClearCartAsync(userId);
        return Ok(result);
    }
}
