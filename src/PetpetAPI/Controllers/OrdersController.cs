using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs.Common;
using PetpetAPI.DTOs.Orders;
using PetpetAPI.Services;
using System.Security.Claims;

namespace PetpetAPI.Controllers;

/// <summary>
/// Order management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Create a new order from current cart items
    /// </summary>
    /// <param name="createOrderDto">Order creation data</param>
    /// <returns>Created order details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _orderService.CreateOrderAsync(userId, createOrderDto);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Get current user's orders with filtering and pagination
    /// </summary>
    /// <param name="filter">Order filter parameters</param>
    /// <returns>Paginated list of user orders</returns>
    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetMyOrders([FromQuery] OrderFilterDto filter)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _orderService.GetUserOrdersAsync(userId, filter);
        return Ok(result);
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    /// <param name="filter">Order filter parameters</param>
    /// <returns>Paginated list of all orders</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetAllOrders([FromQuery] OrderFilterDto filter)
    {
        var result = await _orderService.GetAllOrdersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var result = await _orderService.GetOrderByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        // Check if user owns the order or is admin
        if (result.Data!.UserId != userId && !userRoles.Contains("Admin"))
            return Forbid();

        return Ok(result);
    }

    /// <summary>
    /// Update order status (Admin only)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateOrderStatusDto">New order status</param>
    /// <returns>Updated order details</returns>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
