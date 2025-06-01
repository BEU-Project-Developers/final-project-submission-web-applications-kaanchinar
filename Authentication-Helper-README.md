# Authentication Helper Service

This project includes a comprehensive Authentication Helper Service (`IAuthenticationHelper`) that provides centralized user authentication, role checking, and session management utilities.

## Features

### ✅ User Authentication

- Validate user authentication from JWT tokens
- Extract user ID from claims
- Check user existence and active status

### ✅ Role-Based Authorization

- Check if user has specific roles
- Admin role validation
- Resource ownership verification

### ✅ Session Management

- Centralized authentication validation
- User context management across controllers
- Database user lookup utilities

## Usage Examples

### Basic Authentication Check

```csharp
public class MyController : ControllerBase
{
    private readonly IAuthenticationHelper _authHelper;

    public MyController(IAuthenticationHelper authHelper)
    {
        _authHelper = authHelper;
    }

    [HttpGet]
    public async Task<ActionResult> GetData()
    {
        // Validate authentication and get user ID
        var authResult = _authHelper.ValidateAuthentication(User);
        if (!authResult.Success)
            return Unauthorized(authResult);

        var userId = authResult.Data!; // User ID is guaranteed to be available
        // ... continue with business logic
    }
}
```

### Role Checking

```csharp
[HttpGet("admin-only")]
public ActionResult GetAdminData()
{
    var authResult = _authHelper.ValidateAuthentication(User);
    if (!authResult.Success)
        return Unauthorized(authResult);

    // Check if user is admin
    if (!_authHelper.IsAdmin(User))
        return Forbid();

    // Admin-only logic here
}

[HttpGet("custom-role")]
public ActionResult GetCustomRoleData()
{
    // Check for specific role
    if (!_authHelper.HasRole(User, "Manager"))
        return Forbid();

    // Manager role logic here
}
```

### Resource Ownership Validation

```csharp
[HttpGet("orders/{orderId}")]
public async Task<ActionResult> GetOrder(int orderId)
{
    var authResult = _authHelper.ValidateAuthentication(User);
    if (!authResult.Success)
        return Unauthorized(authResult);

    var order = await _orderService.GetOrderByIdAsync(orderId);
    if (!order.Success)
        return NotFound(order);

    // Check if user owns the resource or is admin
    if (!_authHelper.CanAccessResource(User, order.Data!.UserId))
        return Forbid();

    return Ok(order);
}
```

### User Information Retrieval

```csharp
[HttpGet("profile")]
public async Task<ActionResult> GetProfile()
{
    var authResult = _authHelper.ValidateAuthentication(User);
    if (!authResult.Success)
        return Unauthorized(authResult);

    // Get user from database
    var user = await _authHelper.GetUserByIdAsync(authResult.Data!);
    var userRoles = await _authHelper.GetUserRolesAsync(authResult.Data!);
    var isActive = await _authHelper.IsUserActiveAsync(authResult.Data!);

    return Ok(new { user, userRoles, isActive });
}
```

## Available Methods

### Authentication Methods

- `ValidateAuthentication(ClaimsPrincipal user)` - Validates authentication and returns user ID
- `GetCurrentUserId(ClaimsPrincipal user)` - Extracts user ID from claims
- `GetCurrentUserRoles(ClaimsPrincipal user)` - Gets roles from current user claims

### Role Checking Methods

- `HasRole(ClaimsPrincipal user, string role)` - Checks if user has specific role
- `IsAdmin(ClaimsPrincipal user)` - Checks if user is admin
- `CanAccessResource(ClaimsPrincipal user, string resourceUserId)` - Checks resource ownership or admin access

### Database Methods

- `GetUserByIdAsync(string userId)` - Gets user entity from database
- `IsUserActiveAsync(string userId)` - Checks if user exists and is active
- `GetUserRolesAsync(string userId)` - Gets user roles from database

## Benefits

1. **Centralized Authentication Logic** - No more repeated `User.FindFirst(ClaimTypes.NameIdentifier)` calls
2. **Consistent Error Handling** - Standardized authentication failure responses
3. **Type Safety** - Guaranteed non-null user IDs after validation
4. **Maintainability** - Single source of truth for authentication logic
5. **Testability** - Easy to mock and test authentication scenarios
6. **Performance** - Efficient role checking and user lookup methods

## Integration

The service is automatically registered in the DI container:

```csharp
// Program.cs
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
```

## Example Controllers Using the Helper

- `CartController` - Demonstrates basic authentication validation
- `OrdersController` - Shows resource ownership checking
- `UserManagementController` - Example implementation with all features

## Migration from Manual Authentication

**Before (Manual):**

```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (string.IsNullOrEmpty(userId))
    return Unauthorized();
```

**After (With Helper):**

```csharp
var authResult = _authHelper.ValidateAuthentication(User);
if (!authResult.Success)
    return Unauthorized(authResult);

var userId = authResult.Data!; // Guaranteed non-null
```

This approach provides better error messages, consistent responses, and eliminates null-checking boilerplate code.
