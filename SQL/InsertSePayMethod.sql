-- =====================================================
-- Script: Thêm phương thức thanh toán SePay
-- Mục đích: Kích hoạt chức năng thanh toán qua mã VietQR SePay
-- =====================================================

USE [YukiSoraShopDB];  -- Đảm bảo thay đổi tên DB nếu bạn dùng tên khác
GO

PRINT '========== CHECKING SEPAY PAYMENT METHOD ==========';

IF NOT EXISTS (SELECT 1 FROM PaymentMethods WHERE Name = 'SePay')
BEGIN
    PRINT 'Inserting new SePay payment method...';
    INSERT INTO PaymentMethods (Name, Description, IsActive, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, IsDeleted)
    VALUES (
        'SePay',
        N'Thanh toán tự động bằng mã VietQR qua hệ thống SePay',
        1,              -- IsActive
        GETUTCDATE(),   -- CreatedAt
        'system',       -- CreatedBy
        GETUTCDATE(),   -- ModifiedAt
        'system',       -- ModifiedBy
        0               -- IsDeleted
    );
    PRINT 'SePay payment method inserted successfully.';
END
ELSE
BEGIN
    PRINT 'SePay payment method already exists. Ensuring it is active...';
    
    UPDATE PaymentMethods 
    SET 
        IsActive = 1,
        IsDeleted = 0,
        ModifiedAt = GETUTCDATE(),
        ModifiedBy = 'system'
    WHERE Name = 'SePay';
    
    PRINT 'SePay payment method updated and activated.';
END
GO

PRINT '========== SCRIPT COMPLETED ==========';
