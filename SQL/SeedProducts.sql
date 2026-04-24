-- Seed Categories
INSERT INTO "Categories" ("CategoryName", "Description", "IsActive", "IsDeleted", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy")
VALUES 
('Laptop', 'Máy tính xách tay các loại', true, false, NOW(), 'System', NOW(), 'System'),
('Điện thoại', 'Điện thoại thông minh', true, false, NOW(), 'System', NOW(), 'System'),
('Phụ kiện', 'Phụ kiện máy tính, điện thoại', true, false, NOW(), 'System', NOW(), 'System')
ON CONFLICT ("CategoryName") DO NOTHING;

-- Seed Products
-- Note: Assuming the Category IDs are 1, 2, 3 respectively.
INSERT INTO "Products" ("CategoryId", "CategoryName", "ProductName", "Description", "Price", "StockQuantity", "IsDeleted", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy")
VALUES 
(1, 'Laptop', 'MacBook Pro 14 M3', 'MacBook Pro 14 inch chip M3 mới nhất', 39990000, 50, false, NOW(), 'System', NOW(), 'System'),
(1, 'Laptop', 'Dell XPS 13 Plus', 'Dell XPS 13 viền mỏng thiết kế sang trọng', 35990000, 30, false, NOW(), 'System', NOW(), 'System'),
(1, 'Laptop', 'ThinkPad X1 Carbon Gen 11', 'Laptop doanh nhân siêu bền', 41990000, 20, false, NOW(), 'System', NOW(), 'System'),

(2, 'Điện thoại', 'iPhone 15 Pro Max', 'Apple iPhone 15 Pro Max 256GB', 29990000, 100, false, NOW(), 'System', NOW(), 'System'),
(2, 'Điện thoại', 'Samsung Galaxy S24 Ultra', 'Samsung S24 Ultra tích hợp AI', 31990000, 80, false, NOW(), 'System', NOW(), 'System'),

(3, 'Phụ kiện', 'Chuột Logitech MX Master 3S', 'Chuột công thái học cao cấp', 2490000, 200, false, NOW(), 'System', NOW(), 'System'),
(3, 'Phụ kiện', 'Bàn phím cơ Keychron K8 Pro', 'Bàn phím cơ không dây', 2190000, 150, false, NOW(), 'System', NOW(), 'System'),
(3, 'Phụ kiện', 'Tai nghe Sony WH-1000XM5', 'Tai nghe chống ồn chủ động', 7490000, 60, false, NOW(), 'System', NOW(), 'System')
ON CONFLICT DO NOTHING;

-- Seed ProductDetails (Variants)
-- MacBook Pro 14 M3 details
INSERT INTO "ProductDetails" ("ProductId", "Color", "Size", "Material", "Origin", "ImageUrl", "Description", "AdditionalPrice", "IsDeleted", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy")
VALUES 
(1, 'Space Black', '14 inch', 'Nhôm', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/44/316928/apple-macbook-pro-14-inch-m3-pro-2023-den-thumb-600x600.jpg', 'Bản tiêu chuẩn Space Black', 0, false, NOW(), 'System', NOW(), 'System'),
(1, 'Silver', '14 inch', 'Nhôm', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/44/316928/apple-macbook-pro-14-inch-m3-pro-2023-bac-thumb-600x600.jpg', 'Bản tiêu chuẩn Silver', 0, false, NOW(), 'System', NOW(), 'System'),

(4, 'Titan Tự nhiên', '256GB', 'Titanium', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-titan-thumb-600x600.jpg', 'iPhone 15 Pro Max màu titan tự nhiên', 0, false, NOW(), 'System', NOW(), 'System'),
(4, 'Titan Tự nhiên', '512GB', 'Titanium', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/42/305658/iphone-15-pro-max-blue-titan-thumb-600x600.jpg', 'iPhone 15 Pro Max bản 512GB', 5000000, false, NOW(), 'System', NOW(), 'System'),

(6, 'Đen', 'Tiêu chuẩn', 'Nhựa', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/86/282885/chuot-bluetooth-logitech-mx-master-3s-thumb-600x600.jpeg', 'Logitech MX Master 3S Đen', 0, false, NOW(), 'System', NOW(), 'System');
