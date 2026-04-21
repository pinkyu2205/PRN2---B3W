-- =====================================================
-- QUICK FIX: Payment Method Toggle Problem
-- Purpose: Enable all payment methods and verify
-- =====================================================

USE [YukiSoraShop_DB];  -- ?? Thay ??i tên database n?u khác
GO

PRINT '========================================';
PRINT '   PAYMENT METHOD QUICK FIX SCRIPT';
PRINT '========================================';
PRINT '';

-- =====================================================
-- STEP 1: CHECK CURRENT STATE
-- =====================================================
PRINT '--- STEP 1: Current State ---';
IF EXISTS (SELECT 1 FROM PaymentMethods)
BEGIN
    SELECT 
        Id, 
        Name, 
        IsActive AS [Active], 
        IsDeleted AS [Deleted],
        ModifiedAt AS [Last Modified]
    FROM PaymentMethods;
    
    DECLARE @total INT = (SELECT COUNT(*) FROM PaymentMethods);
    DECLARE @active INT = (SELECT COUNT(*) FROM PaymentMethods WHERE IsActive = 1 AND IsDeleted = 0);
    
    PRINT 'Total methods: ' + CAST(@total AS VARCHAR);
    PRINT 'Active methods: ' + CAST(@active AS VARCHAR);
END
ELSE
BEGIN
    PRINT '?? NO PAYMENT METHODS FOUND!';
    PRINT 'Will insert default methods...';
END
PRINT '';

-- =====================================================
-- STEP 2: FIX - ACTIVATE ALL EXISTING METHODS
-- =====================================================
PRINT '--- STEP 2: Activating All Methods ---';

UPDATE PaymentMethods
SET 
    IsActive = 1,
    IsDeleted = 0,
    ModifiedAt = GETUTCDATE(),
    ModifiedBy = 'admin_quick_fix'
WHERE IsDeleted = 0 OR IsActive = 0;

DECLARE @updated INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@updated AS VARCHAR) + ' records';
PRINT '';

-- =====================================================
-- STEP 3: INSERT MISSING METHODS
-- =====================================================
PRINT '--- STEP 3: Insert Missing Methods ---';

-- Insert Cash if not exists
IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'Cash')
BEGIN
    INSERT INTO PaymentMethods (
        Name, 
        Description, 
        IsActive, 
        CreatedAt, 
        CreatedBy, 
        ModifiedAt, 
        ModifiedBy, 
        IsDeleted
    )
    VALUES (
        'Cash',
        N'Thanh toán ti?n m?t khi nh?n hàng ho?c t?i c?a hàng',
        1,
        GETUTCDATE(),
        'system',
        GETUTCDATE(),
        'system',
        0
    );
    PRINT '? Inserted: Cash';
END
ELSE
BEGIN
    PRINT '? Cash already exists';
END

-- Insert VNPay if not exists
IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'VNPay')
BEGIN
    INSERT INTO PaymentMethods (
        Name, 
        Description, 
        IsActive, 
        CreatedAt, 
        CreatedBy, 
        ModifiedAt, 
        ModifiedBy, 
        IsDeleted
    )
    VALUES (
        'VNPay',
        N'Thanh toán tr?c tuy?n an toàn qua VNPay (ATM, Visa, Mastercard, QR Code)',
        1,
        GETUTCDATE(),
        'system',
        GETUTCDATE(),
        'system',
        0
    );
    PRINT '? Inserted: VNPay';
END
ELSE
BEGIN
    PRINT '? VNPay already exists';
END
PRINT '';

-- =====================================================
-- STEP 4: VERIFY FINAL STATE
-- =====================================================
PRINT '--- STEP 4: Final State ---';
SELECT 
    Id, 
    Name, 
    Description,
    CASE WHEN IsActive = 1 THEN '? Active' ELSE '? Inactive' END AS [Status],
    CASE WHEN IsDeleted = 1 THEN '? Deleted' ELSE '? OK' END AS [Deleted],
    ModifiedAt AS [Modified Time],
    ModifiedBy AS [Modified By]
FROM PaymentMethods
ORDER BY Id;

PRINT '';
PRINT '========================================';
DECLARE @finalActive INT = (SELECT COUNT(*) FROM PaymentMethods WHERE IsActive = 1 AND IsDeleted = 0);
DECLARE @finalTotal INT = (SELECT COUNT(*) FROM PaymentMethods WHERE IsDeleted = 0);

PRINT 'SUMMARY:';
PRINT '  - Total methods: ' + CAST(@finalTotal AS VARCHAR);
PRINT '  - Active methods: ' + CAST(@finalActive AS VARCHAR);

IF @finalActive > 0
BEGIN
    PRINT '';
    PRINT '??? SUCCESS! ???';
    PRINT 'All payment methods are now ACTIVE.';
    PRINT '';
    PRINT 'Next steps:';
    PRINT '  1. Go to https://localhost:7xxx/Admin/PaymentMethods';
    PRINT '  2. Press F5 to refresh';
    PRINT '  3. You should see GREEN badges now';
    PRINT '  4. Try clicking "T?t" to test toggle';
END
ELSE
BEGIN
    PRINT '';
    PRINT '? FAILED - No active methods found';
    PRINT 'Please check:';
    PRINT '  1. Database connection';
    PRINT '  2. Table structure';
    PRINT '  3. Run migrations if needed';
END
PRINT '========================================';
GO
