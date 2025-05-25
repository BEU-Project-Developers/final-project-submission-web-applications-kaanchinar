using Microsoft.EntityFrameworkCore;
using PetpetAPI.Data;
using PetpetAPI.DTOs.Common;
using PetpetAPI.DTOs.Orders;
using PetpetAPI.Models;

namespace PetpetAPI.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto createOrderDto);
    Task<ApiResponse<PagedResult<OrderDto>>> GetUserOrdersAsync(string userId, OrderFilterDto filter);
    Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(OrderFilterDto filter);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int orderId);
    Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto createOrderDto)
    {
        // Get cart items
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            return new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Cart is empty"
            };
        }

        // Check stock availability
        foreach (var cartItem in cartItems)
        {
            if (cartItem.Product.StockQuantity < cartItem.Quantity)
            {
                return new ApiResponse<OrderDto>
                {
                    Success = false,
                    Message = $"Insufficient stock for product: {cartItem.Product.Name}"
                };
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                TotalAmount = cartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                Status = OrderStatus.Waiting,
                ShippingAddress = createOrderDto.ShippingAddress,
                Notes = createOrderDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items and update stock
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price,
                    TotalPrice = cartItem.Product.Price * cartItem.Quantity
                };

                orderItems.Add(orderItem);

                // Update product stock
                cartItem.Product.StockQuantity -= cartItem.Quantity;
                cartItem.Product.UpdatedAt = DateTime.UtcNow;
            }

            _context.OrderItems.AddRange(orderItems);

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetOrderByIdAsync(order.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to create order"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetUserOrdersAsync(string userId, OrderFilterDto filter)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Where(o => o.UserId == userId);

        return await GetOrdersAsync(query, filter);
    }

    public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersAsync(OrderFilterDto filter)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Include(o => o.User);

        return await GetOrdersAsync(query, filter);
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images)
            .Where(o => o.Id == orderId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                ShippingAddress = o.ShippingAddress,
                Notes = o.Notes,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    ProductImageUrl = oi.Product.Images.FirstOrDefault(i => i.IsPrimary) != null
                        ? oi.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                        : oi.Product.Images.FirstOrDefault() != null
                            ? oi.Product.Images.FirstOrDefault()!.ImageUrl
                            : ""
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Order not found"
            };
        }

        return new ApiResponse<OrderDto>
        {
            Success = true,
            Message = "Order retrieved successfully",
            Data = order
        };
    }

    public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            return new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Order not found"
            };
        }

        order.Status = updateOrderStatusDto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(orderId);
    }

    private async Task<ApiResponse<PagedResult<OrderDto>>> GetOrdersAsync(IQueryable<Order> query, OrderFilterDto filter)
    {
        // Apply filters
        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                ShippingAddress = o.ShippingAddress,
                Notes = o.Notes,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    ProductImageUrl = oi.Product.Images.FirstOrDefault(i => i.IsPrimary) != null
                        ? oi.Product.Images.FirstOrDefault(i => i.IsPrimary)!.ImageUrl
                        : oi.Product.Images.FirstOrDefault() != null
                            ? oi.Product.Images.FirstOrDefault()!.ImageUrl
                            : ""
                }).ToList()
            })
            .ToListAsync();

        var result = new PagedResult<OrderDto>
        {
            Items = orders,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };

        return new ApiResponse<PagedResult<OrderDto>>
        {
            Success = true,
            Message = "Orders retrieved successfully",
            Data = result
        };
    }

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
