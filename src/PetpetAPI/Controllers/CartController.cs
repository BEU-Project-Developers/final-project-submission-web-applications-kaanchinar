using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Cart;
using PetpetAPI.DTOs.Common;
using PetpetAPI.Services;

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
    private readonly IAuthenticationHelper _authHelper;

    public CartController(ICartService cartService, IAuthenticationHelper authHelper)
    {
        _cartService = cartService;
        _authHelper = authHelper;
    }

    /// <summary>
    /// Get current user's shopping cart
    /// </summary>
    /// <returns>Cart summary with items</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCart()
    {
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var result = await _cartService.GetCartAsync(authResult.Data!);
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
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var result = await _cartService.AddToCartAsync(authResult.Data!, addToCartDto);
        
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
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var result = await _cartService.UpdateCartItemAsync(authResult.Data!, cartItemId, updateCartItemDto);
        
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
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var result = await _cartService.RemoveFromCartAsync(authResult.Data!, cartItemId);
        
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
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var result = await _cartService.ClearCartAsync(authResult.Data!);
        return Ok(result);
    }
}
