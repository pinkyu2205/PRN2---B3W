-- =====================================================
-- Script: Insert và Kích ho?t Payment Methods
-- M?c ?ích: Fix l?i không th? thanh toán do thi?u payment method
-- Date: 2025-01-10
-- =====================================================

USE [YourDatabaseName];  -- Thay th? b?ng tên database th?c t?
GO

-- =====================================================
-- B??C 1: Ki?m tra tình tr?ng hi?n t?i
-- =====================================================
PRINT '========== CHECKING CURRENT PAYMENT METHODS ==========';

SELECT 
    Id, 
    Name, 
    Description,
    IsActive, 
    IsDeleted,
    ModifiedAt,
    ModifiedBy
FROM PaymentMethods
ORDER BY Id;

-- Count active methods
DECLARE @activeCount INT;
SELECT @activeCount = COUNT(*) FROM PaymentMethods WHERE IsActive = 1 AND IsDeleted = 0;
PRINT 'Active payment methods: ' + CAST(@activeCount AS VARCHAR(10));

-- =====================================================
-- B??C 2: T?o b?ng n?u ch?a t?n t?i (safety check)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentMethods')
BEGIN
    PRINT 'ERROR: PaymentMethods table does not exist!';
    PRINT 'Please run migrations first: dotnet ef database update';
    RETURN;
END

-- =====================================================
-- B??C 3: Clean up data c? (OPTIONAL - uncomment n?u mu?n reset)
-- =====================================================
-- WARNING: This will delete all existing payment methods and related data!
-- Uncomment below lines only if you want to start fresh

/*
PRINT '========== CLEANING OLD DATA (COMMENTED OUT) ==========';
-- First, soft delete (set IsDeleted = 1)
UPDATE PaymentMethods SET IsDeleted = 1, ModifiedAt = GETUTCDATE(), ModifiedBy = 'system';

-- Or hard delete (uncomment if really needed)
-- DELETE FROM Payments WHERE PaymentMethodId IN (SELECT Id FROM PaymentMethods);
-- DELETE FROM PaymentMethods;

PRINT 'Old data cleaned.';
*/

-- =====================================================
-- B??C 4: Insert Cash Payment Method
-- =====================================================
PRINT '========== INSERTING/UPDATING CASH PAYMENT METHOD ==========';

IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'Cash' AND IsDeleted = 0)
BEGIN
    -- Check if soft-deleted Cash exists
    IF EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'Cash' AND IsDeleted = 1)
    BEGIN
        PRINT 'Found soft-deleted Cash method, restoring...';
        UPDATE PaymentMethods 
        SET 
            IsDeleted = 0,
            IsActive = 1,
            Description = N'Thanh toán ti?n m?t khi nh?n hàng ho?c t?i c?a hàng',
            ModifiedAt = GETUTCDATE(),
            ModifiedBy = 'system'
        WHERE Name = 'Cash' AND IsDeleted = 1;
    END
    ELSE
    BEGIN
        PRINT 'Inserting new Cash payment method...';
        INSERT INTO PaymentMethods (Name, Description, IsActive, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, IsDeleted)
        VALUES (
            'Cash',
            N'Thanh toán ti?n m?t khi nh?n hàng ho?c t?i c?a hàng',
            1,  -- IsActive
            GETUTCDATE(),
            'system',
            GETUTCDATE(),
            'system',
            0  -- IsDeleted
        );
    END
    PRINT 'Cash payment method ready.';
END
ELSE
BEGIN
    PRINT 'Cash payment method already exists.';
    -- Make sure it's active
    UPDATE PaymentMethods 
    SET 
        IsActive = 1,
        IsDeleted = 0,
        ModifiedAt = GETUTCDATE(),
        ModifiedBy = 'system'
    WHERE Name = 'Cash';
    PRINT 'Cash payment method activated.';
END

-- =====================================================
-- B??C 5: Insert VNPay Payment Method
-- =====================================================
PRINT '========== INSERTING/UPDATING VNPAY PAYMENT METHOD ==========';

IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'VNPay' AND IsDeleted = 0)
BEGIN
    -- Check if soft-deleted VNPay exists
    IF EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'VNPay' AND IsDeleted = 1)
    BEGIN
        PRINT 'Found soft-deleted VNPay method, restoring...';
        UPDATE PaymentMethods 
        SET 
            IsDeleted = 0,
            IsActive = 1,
            Description = N'Thanh toán tr?c tuy?n an toàn qua VNPay (ATM, Visa, Mastercard, QR Code)',
            ModifiedAt = GETUTCDATE(),
            ModifiedBy = 'system'
        WHERE Name = 'VNPay' AND IsDeleted = 1;
    END
    ELSE
    BEGIN
        PRINT 'Inserting new VNPay payment method...';
        INSERT INTO PaymentMethods (Name, Description, IsActive, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, IsDeleted)
        VALUES (
            'VNPay',
            N'Thanh toán tr?c tuy?n an toàn qua VNPay (ATM, Visa, Mastercard, QR Code)',
            1,  -- IsActive
            GETUTCDATE(),
            'system',
            GETUTCDATE(),
            'system',
            0  -- IsDeleted
        );
    END
    PRINT 'VNPay payment method ready.';
END
ELSE
BEGIN
    PRINT 'VNPay payment method already exists.';
    -- Make sure it's active
    UPDATE PaymentMethods 
    SET 
        IsActive = 1,
        IsDeleted = 0,
        ModifiedAt = GETUTCDATE(),
        ModifiedBy = 'system'
    WHERE Name = 'VNPay';
    PRINT 'VNPay payment method activated.';
END

-- =====================================================
-- B??C 6: Verify Results
-- =====================================================
PRINT '========== VERIFICATION ==========';

SELECT 
    Id,
    Name,
    Description,
    IsActive,
    IsDeleted,
    CreatedAt,
    CreatedBy,
    ModifiedAt,
    ModifiedBy
FROM PaymentMethods
WHERE IsDeleted = 0
ORDER BY Name;

-- Check active count
SELECT @activeCount = COUNT(*) FROM PaymentMethods WHERE IsActive = 1 AND IsDeleted = 0;
PRINT '';
PRINT '========== SUMMARY ==========';
PRINT 'Total payment methods: ' + CAST((SELECT COUNT(*) FROM PaymentMethods WHERE IsDeleted = 0) AS VARCHAR(10));
PRINT 'Active payment methods: ' + CAST(@activeCount AS VARCHAR(10));

IF @activeCount > 0
BEGIN
    PRINT '';
    PRINT '? SUCCESS: Payment methods are ready!';
    PRINT 'You can now proceed with checkout in the application.';
END
ELSE
BEGIN
    PRINT '';
    PRINT '? WARNING: No active payment methods found!';
    PRINT 'Please check the script execution or contact support.';
END

-- =====================================================
-- B??C 7: Additional Commands (OPTIONAL)
-- =====================================================

-- Uncomment below to DISABLE a specific payment method
/*
UPDATE PaymentMethods 
SET IsActive = 0, ModifiedAt = GETUTCDATE(), ModifiedBy = 'admin'
WHERE Name = 'VNPay';
PRINT 'VNPay disabled.';
*/

-- Uncomment below to SOFT DELETE a payment method
/*
UPDATE PaymentMethods 
SET IsDeleted = 1, ModifiedAt = GETUTCDATE(), ModifiedBy = 'admin'
WHERE Name = 'Cash';
PRINT 'Cash soft deleted.';
*/

-- Uncomment below to view all payments (for debugging)
/*
SELECT 
    p.Id AS PaymentId,
    o.Id AS OrderId,
    pm.Name AS PaymentMethod,
    p.Amount,
    p.PaymentStatus,
    p.TransactionRef,
    p.CreatedAt
FROM Payments p
LEFT JOIN Orders o ON p.OrderId = o.Id
LEFT JOIN PaymentMethods pm ON p.PaymentMethodId = pm.Id
ORDER BY p.CreatedAt DESC;
*/

GO

PRINT '';
PRINT '========== SCRIPT COMPLETED ==========';
PRINT 'Date: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
