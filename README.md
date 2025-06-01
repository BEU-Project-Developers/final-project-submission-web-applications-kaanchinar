# PetPet API

A comprehensive backend API for an online pet shop and pet adoption platform built with ASP.NET Core 9, PostgreSQL, and Docker.

## Business logic is in BUSINESS.md

## 🚀 Features

### Shop Logic

- **Product Management**: Complete CRUD operations for pet products
- **Categories**: Support for Cats, Dogs, and Other animals
- **Product Filtering**: Filter by price range, category (toys, food, litters, medicines), and more
- **Shopping Cart**: Add, update, remove items from cart
- **Order Management**: Create orders, track order status
- **Inventory Management**: Stock tracking with low stock alerts

### Admin Panel

- **Dashboard**: Total products, inventory value, low stock alerts, recent products
- **Product Management**: Add, edit, delete products with image support
- **Order Management**: View and update order statuses (waiting, processing, completed, withdrawn, rejected)
- **Inventory Tracking**: Monitor stock levels and get low stock alerts

### Authentication & Authorization

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Admin and User roles
- **Refresh Tokens**: Secure token refresh mechanism
- **User Management**: Registration, login, logout functionality

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 9 Web API
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Documentation**: OpenAPI/Swagger with Scalar UI
- **Containerization**: Docker & Docker Compose
- **ORM**: Entity Framework Core with Npgsql

## 📋 Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- Git

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/PetpetAPI.git
cd PetpetAPI
```

### 2. Environment Setup

The application uses Docker Compose for easy setup. All necessary configuration is already included.

### 3. Run with Docker Compose

```bash
# Build and start all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build
```

This will start:

- PostgreSQL database on port 5432
- PetPet API on port 8080

### 4. Access the API

- **API Base URL**: <http://localhost:8080>
- **Swagger UI**: <http://localhost:8080/swagger>
- **Scalar API Documentation**: <http://localhost:8080/api/reference>

### 5. Default Admin Account

- **Email**: <admin@petpet.com>
- **Password**: Admin123!

## 📚 API Documentation

The API includes comprehensive OpenAPI documentation accessible at:

- **Swagger UI**: <http://localhost:8080/swagger>
- **Scalar UI**: <http://localhost:8080/api/reference> (Modern API documentation interface)

## 🗂 Project Structure

``` text
PetpetAPI/
├── src/
│   └── PetpetAPI/
│       ├── Controllers/           # API Controllers
│       ├── Data/                 # Database Context
│       ├── DTOs/                 # Data Transfer Objects
│       │   ├── Auth/
│       │   ├── Products/
│       │   ├── Cart/
│       │   ├── Orders/
│       │   ├── Admin/
│       │   └── Common/
│       ├── Models/               # Entity Models
│       ├── Services/             # Business Logic
│       └── Program.cs            # Application Entry Point
├── Dockerfile                   # Docker configuration
├── docker-compose.yml          # Docker Compose configuration
└── README.md
```

## 🔑 Authentication

### Registration

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Password123!"
}
```

### Using JWT Token

Include the JWT token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

## 📦 Main API Endpoints

### Products

- `GET /api/products` - Get products with filtering
- `GET /api/products/{id}` - Get specific product
- `POST /api/products` - Create product (Admin)
- `PUT /api/products/{id}` - Update product (Admin)
- `DELETE /api/products/{id}` - Delete product (Admin)

### Cart

- `GET /api/cart` - Get user's cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items/{id}` - Update cart item
- `DELETE /api/cart/items/{id}` - Remove cart item

### Orders

- `POST /api/orders` - Create order from cart
- `GET /api/orders/my-orders` - Get user's orders
- `GET /api/orders` - Get all orders (Admin)
- `PUT /api/orders/{id}/status` - Update order status (Admin)

### Admin

- `GET /api/admin/dashboard/stats` - Dashboard statistics
- `GET /api/admin/products/low-stock` - Low stock products

## 🔧 Configuration

### Database Connection

The connection string is configured in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Database=petpetdb;Username=postgres;Password=postgres"
  }
}
```

### JWT Settings

JWT configuration in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-minimum-32-characters-long",
    "Issuer": "PetpetAPI",
    "Audience": "PetpetAPI-Users",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

## 🐛 Development

### Local Development (without Docker)

1. Install PostgreSQL locally
2. Update connection string in `appsettings.json`
3. Run migrations:

   ```bash
   cd src/PetpetAPI
   dotnet ef database update
   ```

4. Run the application:

   ```bash
   dotnet run
   ```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 📝 Sample Data

The application automatically seeds sample data including:

- Default admin user
- Sample products (cat food, dog toys, cat litter)
- Product images
- User and Admin roles

## 🚢 Deployment

### Docker Production Deployment

```bash
# Build production image
docker build -t petpetapi:latest .

# Run with external database
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=petpetdb;Username=user;Password=pass" \
  petpetapi:latest
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support, email <support@petpet.com> or create an issue in the repository.

---

**PetPet API** - Making pet care accessible and convenient! 🐾
