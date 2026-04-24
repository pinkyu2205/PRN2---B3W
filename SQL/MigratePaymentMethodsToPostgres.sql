UPDATE "PaymentMethods" SET "IsActive" = false, "IsDeleted" = true WHERE "Name" = 'VNPay';

INSERT INTO "PaymentMethods" ("Name", "Description", "IsActive", "IsDeleted", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy")
VALUES ('SePay', 'Thanh toán qua mã VietQR tự động bằng SePay', true, false, NOW(), 'System', NOW(), 'System');
