-- Seed ProductDetails (Variants) for the rest of the products
INSERT INTO "ProductDetails" ("ProductId", "Color", "Size", "Material", "Origin", "ImageUrl", "Description", "AdditionalPrice", "IsDeleted", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy")
VALUES 
-- Product 2: Dell XPS 13 Plus
(2, 'Bạc', '13.4 inch', 'Nhôm', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/44/282827/dell-xps-13-plus-9320-i7-70295789-thumb-600x600.jpg', 'Bản tiêu chuẩn', 0, false, NOW(), 'System', NOW(), 'System'),

-- Product 3: ThinkPad X1 Carbon Gen 11
(3, 'Đen', '14 inch', 'Carbon Fiber', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/44/302867/lenovo-thinkpad-x1-carbon-gen-10-i7-21cb00a8vn-thumb-600x600.jpg', 'Bản tiêu chuẩn', 0, false, NOW(), 'System', NOW(), 'System'),

-- Product 5: Samsung Galaxy S24 Ultra
(5, 'Xám Titan', '256GB', 'Titanium', 'Hàn Quốc', 'https://cdn.tgdd.vn/Products/Images/42/307174/samsung-galaxy-s24-ultra-grey-thumb-600x600.jpg', 'S24 Ultra tiêu chuẩn', 0, false, NOW(), 'System', NOW(), 'System'),

-- Product 7: Bàn phím cơ Keychron K8 Pro
(7, 'Đen', 'TKL', 'Nhựa/Nhôm', 'Trung Quốc', 'https://cdn.tgdd.vn/Products/Images/86/303423/ban-phim-co-khong-day-keychron-k8-pro-rgb-thumb-1-600x600.jpg', 'Bàn phím cơ', 0, false, NOW(), 'System', NOW(), 'System'),

-- Product 8: Tai nghe Sony WH-1000XM5
(8, 'Đen', 'Over-ear', 'Nhựa', 'Nhật Bản', 'https://cdn.tgdd.vn/Products/Images/54/283307/tai-nghe-chup-tai-bluetooth-sony-wh-1000xm5-den-thumb-600x600.jpg', 'Tai nghe chống ồn', 0, false, NOW(), 'System', NOW(), 'System')
ON CONFLICT DO NOTHING;
