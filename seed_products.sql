-- Seed Product Catalog with 18 new products
-- This script adds products based on the JSON data provided

-- Insert Products (starting from ID 5 since 1-4 already exist)
INSERT INTO "Products" ("Id", "Name", "Description", "Price", "OriginalPrice", "Brand", "StockQuantity", "LowStockThreshold", "Section", "Category", "State", "CreatedAt", "UpdatedAt", "IsActive")
VALUES 
-- Cat Products (IDs 5-10)
(5, 'Premium Cat Food - Salmon', 'High-quality salmon-based cat food for all life stages.', 24.99, 29.99, 'PetNutrition', 100, 10, 1, 2, 1, NOW(), NULL, true),
(6, 'Interactive Feather Wand', 'Engaging toy to stimulate your cat''s hunting instincts.', 12.99, 12.99, 'PlayPets', 80, 15, 1, 1, 2, NOW(), NULL, true),
(7, 'Cozy Cat Cave Bed', 'Soft, warm bed that provides security and comfort.', 34.99, 44.99, 'ComfyPets', 50, 10, 1, 5, 1, NOW(), NULL, true),
(8, 'Odor Control Clumping Litter', 'Superior odor control with minimal dust.', 18.99, 18.99, 'CleanPets', 75, 15, 1, 3, 1, NOW(), NULL, true),
(9, 'Multi-level Cat Scratcher Tower', 'Durable cardboard scratcher with multiple levels for play.', 29.99, 39.99, 'ScratchCo', 40, 8, 1, 5, 2, NOW(), NULL, true),
(10, 'Hairball Control Supplement', 'Natural supplement to reduce hairballs and support digestion.', 15.99, 15.99, 'PetHealth', 60, 12, 1, 4, 1, NOW(), NULL, true),

-- Dog Products (IDs 11-16)
(11, 'Grain-Free Dog Food - Chicken', 'Nutritious grain-free formula with real chicken.', 39.99, 49.99, 'PetNutrition', 90, 15, 2, 2, 1, NOW(), NULL, true),
(12, 'Durable Chew Toy', 'Long-lasting rubber toy for aggressive chewers.', 14.99, 14.99, 'ToughPlay', 120, 20, 2, 1, 2, NOW(), NULL, true),
(13, 'Orthopedic Dog Bed - Large', 'Supportive memory foam bed for joint relief.', 79.99, 99.99, 'ComfyPets', 30, 5, 2, 5, 1, NOW(), NULL, true),
(14, 'Reflective Safety Collar', 'Adjustable collar with reflective stitching for night visibility.', 18.99, 18.99, 'SafePets', 85, 15, 2, 5, 1, NOW(), NULL, true),
(15, 'Retractable Dog Leash', 'Durable leash with comfortable grip and locking mechanism.', 24.99, 29.99, 'WalkPro', 70, 12, 2, 5, 2, NOW(), NULL, true),
(16, 'Natural Dog Treats - Assorted', 'Healthy, grain-free treats made with real meat.', 12.99, 12.99, 'TastyPets', 150, 25, 2, 2, 1, NOW(), NULL, true),

-- Other Animals Products (IDs 17-22)
(17, 'Premium Bird Seed Mix', 'Nutritionally complete seed mix for small to medium birds.', 9.99, 12.99, 'FeatherFriends', 80, 15, 3, 2, 1, NOW(), NULL, true),
(18, '10-Gallon Aquarium Starter Kit', 'Complete setup with filter, lighting, and decorations.', 59.99, 79.99, 'AquaLife', 25, 5, 3, 5, 2, NOW(), NULL, true),
(19, 'Deluxe Hamster Habitat', 'Spacious cage with tunnels, wheel, and accessories.', 44.99, 44.99, 'SmallPets', 35, 8, 3, 5, 1, NOW(), NULL, true),
(20, 'Reptile Heat Lamp', 'Adjustable lamp to provide essential heat for reptiles.', 29.99, 34.99, 'ReptileCare', 45, 10, 3, 5, 1, NOW(), NULL, true),
(21, 'Premium Rabbit Pellets', 'Nutritionally balanced food for rabbits of all ages.', 14.99, 14.99, 'BunnyBites', 90, 18, 3, 2, 2, NOW(), NULL, true),
(22, 'Natural Paper Bedding', 'Soft, absorbent bedding made from recycled paper.', 12.99, 15.99, 'CozyPets', 65, 12, 3, 5, 1, NOW(), NULL, true);

-- Insert Product Images (starting from ID 4 since 1-3 already exist)
INSERT INTO "ProductImages" ("Id", "ProductId", "ImageUrl", "AltText", "DisplayOrder", "IsPrimary", "CreatedAt")
VALUES 
-- Cat Product Images
(4, 5, 'https://images.unsplash.com/photo-1655210913315-e8147faf7600?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Premium Cat Food - Salmon', 1, true, NOW()),
(5, 6, 'https://images.unsplash.com/photo-1640529410767-d9e02123d789?q=80&w=1800&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Interactive Feather Wand', 1, true, NOW()),
(6, 7, 'https://images.unsplash.com/photo-1573682127988-f67136e7f12a?q=80&w=2074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Cozy Cat Cave Bed', 1, true, NOW()),
(7, 8, 'https://images.unsplash.com/photo-1727510152470-84074e7acd9b?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Odor Control Clumping Litter', 1, true, NOW()),
(8, 9, 'https://images.unsplash.com/photo-1636543459628-12fbfb7478c6?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Multi-level Cat Scratcher Tower', 1, true, NOW()),
(9, 10, 'https://images.unsplash.com/photo-1729703551891-d4f2d9ad1ebb?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Hairball Control Supplement', 1, true, NOW()),

-- Dog Product Images
(10, 11, 'https://images.unsplash.com/photo-1589924691995-400dc9ecc119?q=80&w=2074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Grain-Free Dog Food - Chicken', 1, true, NOW()),
(11, 12, 'https://images.unsplash.com/photo-1601758228041-f3b2795255f1?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Durable Chew Toy', 1, true, NOW()),
(12, 13, 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?q=80&w=2064&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Orthopedic Dog Bed - Large', 1, true, NOW()),
(13, 14, 'https://images.unsplash.com/photo-1605568427561-40dd23c2acea?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Reflective Safety Collar', 1, true, NOW()),
(14, 15, 'https://images.unsplash.com/photo-1601758165737-db8d20852565?q=80&w=2073&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Retractable Dog Leash', 1, true, NOW()),
(15, 16, 'https://images.unsplash.com/photo-1589941013453-ec89f33b5e95?q=80&w=2085&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Natural Dog Treats - Assorted', 1, true, NOW()),

-- Other Animals Product Images
-- Other Animals Product Images
(16, 17, 'https://images.unsplash.com/photo-1452570053594-1b985d6ea890?q=80&w=2106&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Premium Bird Seed Mix', 1, true, NOW()),
(17, 18, 'https://images.unsplash.com/photo-1524704654690-b56c05c78a00?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', '10-Gallon Aquarium Starter Kit', 1, true, NOW()),
(18, 19, 'https://images.unsplash.com/photo-1425082661705-1834bfd09dca?q=80&w=2076&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Deluxe Hamster Habitat', 1, true, NOW()),
(19, 20, 'https://images.unsplash.com/photo-1567515004624-219c11d31f2e?q=80&w=2112&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Reptile Heat Lamp', 1, true, NOW()),
(20, 21, 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?q=80&w=2064&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Premium Rabbit Pellets', 1, true, NOW()),
(21, 22, 'https://images.unsplash.com/photo-1452570053594-1b985d6ea890?q=80&w=2106&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', 'Natural Paper Bedding', 1, true, NOW());

-- Update the sequence counters to prevent ID conflicts
SELECT setval('"Products_Id_seq"', (SELECT MAX("Id") FROM "Products"));
SELECT setval('"ProductImages_Id_seq"', (SELECT MAX("Id") FROM "ProductImages"));

-- Verify the data was inserted
SELECT COUNT(*) as "Total Products" FROM "Products";
SELECT COUNT(*) as "Total Product Images" FROM "ProductImages";

-- Show a sample of the new products
SELECT "Id", "Name", "Brand", "Price", "Section", "Category" 
FROM "Products" 
WHERE "Id" >= 4 
ORDER BY "Id";
