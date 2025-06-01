using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PetpetAPI.Models;

namespace PetpetAPI.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewHelpfulness> ReviewHelpfulness { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Product configuration
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Brand).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.OriginalPrice).HasPrecision(18, 2);
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.Section);
            entity.HasIndex(p => p.Category);
        });

        // ProductImage configuration
        builder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(pi => pi.AltText).HasMaxLength(200);
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem configuration
        builder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.HasOne(ci => ci.User)
                  .WithMany(u => u.CartItems)
                  .HasForeignKey(ci => ci.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ci => ci.Product)
                  .WithMany(p => p.CartItems)
                  .HasForeignKey(ci => ci.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ci => ci.UserId);
        });

        // Order configuration
        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
            entity.Property(o => o.Notes).HasMaxLength(1000);
            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.UserId);
        });

        // OrderItem configuration
        builder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            entity.Property(oi => oi.TotalPrice).HasPrecision(18, 2);
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
            entity.HasOne(rt => rt.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
        });

        // User configuration
        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        });

        // Review configuration
        builder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Comment).IsRequired().HasMaxLength(1000);
            entity.Property(r => r.Title).HasMaxLength(100);
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Product)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Order)
                  .WithMany(o => o.Reviews)
                  .HasForeignKey(r => r.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(r => r.ProductId);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => new { r.UserId, r.ProductId, r.OrderId }).IsUnique();
        });

        // ReviewHelpfulness configuration
        builder.Entity<ReviewHelpfulness>(entity =>
        {
            entity.HasKey(rh => rh.Id);
            entity.HasOne(rh => rh.User)
                  .WithMany(u => u.ReviewHelpfulness)
                  .HasForeignKey(rh => rh.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rh => rh.Review)
                  .WithMany(r => r.ReviewHelpfulness)
                  .HasForeignKey(rh => rh.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(rh => new { rh.UserId, rh.ReviewId }).IsUnique();
        });

        // Seed data
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        // Seed some initial products for testing
        builder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Premium Cat Food",
                Description = "High-quality nutrition for adult cats",
                Price = 29.99m,
                OriginalPrice = 34.99m,
                Brand = "PetNutrition",
                StockQuantity = 50,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Dog Chew Toy",
                Description = "Durable rubber toy for large dogs",
                Price = 15.99m,
                Brand = "PlayTime",
                StockQuantity = 25,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Cat Litter Premium",
                Description = "Clumping cat litter with odor control",
                Price = 12.99m,
                Brand = "CleanPaws",
                StockQuantity = 5, // Low stock for testing
                LowStockThreshold = 10,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Litters,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            }
        );

        builder.Entity<ProductImage>().HasData(
            new ProductImage
            {
                Id = 1,
                ProductId = 1,
                ImageUrl = "/images/cat-food-premium.jpg",
                AltText = "Premium Cat Food Package",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 2,
                ProductId = 2,
                ImageUrl = "/images/dog-chew-toy.jpg",
                AltText = "Dog Chew Toy",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 3,
                ProductId = 3,
                ImageUrl = "/images/cat-litter-premium.jpg",
                AltText = "Premium Cat Litter",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
