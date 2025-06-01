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
        // Seed comprehensive product catalog
        builder.Entity<Product>().HasData(
            // Existing products (1-3)
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
            },
            
            // New Cat Products (4-9)
            new Product
            {
                Id = 4,
                Name = "Organic Cat Food - Chicken",
                Description = "Made with organic, free-range chicken and wholesome vegetables. Perfect for cats with sensitive stomachs.",
                Price = 34.99m,
                OriginalPrice = 39.99m,
                Brand = "Nature's Best",
                StockQuantity = 45,
                LowStockThreshold = 10,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 5,
                Name = "Interactive Cat Puzzle Feeder",
                Description = "Slow-feeding puzzle toy that challenges your cat mentally while eating. Reduces fast eating and improves digestion.",
                Price = 24.99m,
                Brand = "PuzzlePet",
                StockQuantity = 30,
                LowStockThreshold = 5,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 6,
                Name = "Orthopedic Cat Bed",
                Description = "Memory foam cat bed with removable, washable cover. Perfect for senior cats or those with joint issues.",
                Price = 49.99m,
                OriginalPrice = 59.99m,
                Brand = "ComfortPaw",
                StockQuantity = 20,
                LowStockThreshold = 5,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Accessories,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 7,
                Name = "Catnip Mice Toys (3-pack)",
                Description = "Set of 3 realistic mice filled with premium catnip. Hours of entertainment for your feline friend.",
                Price = 12.99m,
                Brand = "FelineJoy",
                StockQuantity = 60,
                LowStockThreshold = 15,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 8,
                Name = "Natural Clay Cat Litter",
                Description = "100% natural clay litter with excellent odor control. Dust-free and easy to scoop.",
                Price = 18.99m,
                Brand = "EcoClean",
                StockQuantity = 35,
                LowStockThreshold = 8,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Litters,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 9,
                Name = "Cat Grooming Kit",
                Description = "Complete grooming set including slicker brush, nail clippers, and dental care items.",
                Price = 32.99m,
                Brand = "GroomPro",
                StockQuantity = 25,
                LowStockThreshold = 5,
                Section = AnimalSection.Cats,
                Category = ProductCategory.Grooming,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            
            // Dog Products (10-15)
            new Product
            {
                Id = 10,
                Name = "Premium Dog Food - Salmon & Rice",
                Description = "High-protein dog food made with wild-caught salmon and brown rice. Rich in Omega-3 fatty acids.",
                Price = 42.99m,
                OriginalPrice = 47.99m,
                Brand = "WildPack",
                StockQuantity = 40,
                LowStockThreshold = 10,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 11,
                Name = "Rope Tug Toy",
                Description = "Heavy-duty cotton rope toy perfect for tug-of-war and interactive play. Great for dental health.",
                Price = 16.99m,
                Brand = "ToughPlay",
                StockQuantity = 55,
                LowStockThreshold = 15,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 12,
                Name = "Waterproof Dog Bed",
                Description = "Large, waterproof dog bed with memory foam support. Perfect for outdoor use or messy pups.",
                Price = 78.99m,
                OriginalPrice = 89.99m,
                Brand = "OutdoorPet",
                StockQuantity = 15,
                LowStockThreshold = 3,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Accessories,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 13,
                Name = "Tennis Ball Set (6-pack)",
                Description = "High-bounce tennis balls designed specifically for dogs. Bright colors for easy visibility.",
                Price = 19.99m,
                Brand = "BounceMax",
                StockQuantity = 70,
                LowStockThreshold = 20,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 14,
                Name = "Dog Dental Chews",
                Description = "Dental chews that help reduce tartar and freshen breath. Made with natural ingredients.",
                Price = 22.99m,
                Brand = "DentalCare",
                StockQuantity = 45,
                LowStockThreshold = 10,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Medicines,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 15,
                Name = "Professional Dog Shampoo",
                Description = "Gentle, hypoallergenic shampoo for dogs with sensitive skin. Natural ingredients with pleasant scent.",
                Price = 26.99m,
                Brand = "PurePaws",
                StockQuantity = 30,
                LowStockThreshold = 8,
                Section = AnimalSection.Dogs,
                Category = ProductCategory.Grooming,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            
            // Other Animals Products (16-21)
            new Product
            {
                Id = 16,
                Name = "Rabbit Pellet Food",
                Description = "High-fiber pellet food specially formulated for adult rabbits. Contains timothy hay and vegetables.",
                Price = 28.99m,
                Brand = "BunnyBest",
                StockQuantity = 25,
                LowStockThreshold = 5,
                Section = AnimalSection.Other,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 17,
                Name = "Bird Seed Mix - Premium",
                Description = "Gourmet seed mix for songbirds with sunflower seeds, millet, and dried fruits.",
                Price = 15.99m,
                Brand = "FeatherFeast",
                StockQuantity = 50,
                LowStockThreshold = 12,
                Section = AnimalSection.Other,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 18,
                Name = "Hamster Exercise Wheel",
                Description = "Silent spinner exercise wheel for hamsters and small pets. Solid running surface prevents injury.",
                Price = 21.99m,
                Brand = "SmallPet",
                StockQuantity = 35,
                LowStockThreshold = 8,
                Section = AnimalSection.Other,
                Category = ProductCategory.Toys,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 19,
                Name = "Guinea Pig Hay",
                Description = "Premium timothy hay for guinea pigs and rabbits. High in fiber and essential for dental health.",
                Price = 19.99m,
                Brand = "MeadowFresh",
                StockQuantity = 40,
                LowStockThreshold = 10,
                Section = AnimalSection.Other,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 20,
                Name = "Aquarium Fish Food Flakes",
                Description = "Nutritious tropical fish flakes with enhanced color formula. Suitable for all tropical fish species.",
                Price = 12.99m,
                Brand = "AquaLife",
                StockQuantity = 60,
                LowStockThreshold = 15,
                Section = AnimalSection.Other,
                Category = ProductCategory.Food,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 21,
                Name = "Reptile Heat Lamp",
                Description = "UVB heat lamp essential for reptile health. Provides necessary warmth and vitamin D3 synthesis.",
                Price = 45.99m,
                Brand = "ReptileZone",
                StockQuantity = 20,
                LowStockThreshold = 5,
                Section = AnimalSection.Other,
                Category = ProductCategory.Accessories,
                State = ProductState.InStock,
                CreatedAt = DateTime.UtcNow
            }
        );

        builder.Entity<ProductImage>().HasData(
            // Existing images (1-3)
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
            },
            
            // New product images (4-21)
            new ProductImage
            {
                Id = 4,
                ProductId = 4,
                ImageUrl = "/images/organic-cat-food-chicken.jpg",
                AltText = "Organic Cat Food with Chicken",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 5,
                ProductId = 5,
                ImageUrl = "/images/interactive-cat-puzzle-feeder.jpg",
                AltText = "Interactive Cat Puzzle Feeder",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 6,
                ProductId = 6,
                ImageUrl = "/images/orthopedic-cat-bed.jpg",
                AltText = "Orthopedic Cat Bed",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 7,
                ProductId = 7,
                ImageUrl = "/images/catnip-mice-toys.jpg",
                AltText = "Catnip Mice Toys 3-pack",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 8,
                ProductId = 8,
                ImageUrl = "/images/natural-clay-cat-litter.jpg",
                AltText = "Natural Clay Cat Litter",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 9,
                ProductId = 9,
                ImageUrl = "/images/cat-grooming-kit.jpg",
                AltText = "Cat Grooming Kit",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 10,
                ProductId = 10,
                ImageUrl = "/images/premium-dog-food-salmon.jpg",
                AltText = "Premium Dog Food - Salmon & Rice",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 11,
                ProductId = 11,
                ImageUrl = "/images/rope-tug-toy.jpg",
                AltText = "Rope Tug Toy",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 12,
                ProductId = 12,
                ImageUrl = "/images/waterproof-dog-bed.jpg",
                AltText = "Waterproof Dog Bed",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 13,
                ProductId = 13,
                ImageUrl = "/images/tennis-ball-set.jpg",
                AltText = "Tennis Ball Set 6-pack",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 14,
                ProductId = 14,
                ImageUrl = "/images/dog-dental-chews.jpg",
                AltText = "Dog Dental Chews",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 15,
                ProductId = 15,
                ImageUrl = "/images/professional-dog-shampoo.jpg",
                AltText = "Professional Dog Shampoo",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 16,
                ProductId = 16,
                ImageUrl = "/images/rabbit-pellet-food.jpg",
                AltText = "Rabbit Pellet Food",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 17,
                ProductId = 17,
                ImageUrl = "/images/bird-seed-mix-premium.jpg",
                AltText = "Bird Seed Mix - Premium",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 18,
                ProductId = 18,
                ImageUrl = "/images/hamster-exercise-wheel.jpg",
                AltText = "Hamster Exercise Wheel",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 19,
                ProductId = 19,
                ImageUrl = "/images/guinea-pig-hay.jpg",
                AltText = "Guinea Pig Timothy Hay",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 20,
                ProductId = 20,
                ImageUrl = "/images/aquarium-fish-food-flakes.jpg",
                AltText = "Aquarium Fish Food Flakes",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProductImage
            {
                Id = 21,
                ProductId = 21,
                ImageUrl = "/images/reptile-heat-lamp.jpg",
                AltText = "Reptile UVB Heat Lamp",
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
