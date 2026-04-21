# YukiSoraShop

Một ứng dụng web bán hàng được xây dựng bằng ASP.NET Core với kiến trúc Clean Architecture.

## Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

## Cài đặt và chạy ứng dụng

### 1. Clone repository

```bash
git clone <repository-url>
cd YukiSoraShop
```

### 2. Cấu hình Database

#### Cấu hình Connection String

Mở file `YukiSoraShop/appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "YukiSoraShop_DB": "Server=(local);uid=sa;pwd=12345;Database=YukiSoraShopDB;TrustServerCertificate=true;Encrypt=true;"
  }
}
```

**Lưu ý:** Thay đổi các thông tin sau cho phù hợp với môi trường của bạn:
- `Server`: Tên server SQL Server (ví dụ: `(local)`, `localhost`, `.\SQLEXPRESS`)
- `uid`: Username của SQL Server
- `pwd`: Password của SQL Server
- `Database`: Tên database (có thể giữ nguyên `YukiSoraShopDB`)

#### Tạo Database và Migration

1. **Mở Terminal (View -> Terminal)
2. **Chạy lệnh migration để tạo database:**

```bash


# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project YukiSoraShop


# Cập nhật database với migration
dotnet ef database update --project Infrastructure --startup-project YukiSoraShop
```


### 3. Chạy ứng dụng

```bash
# Di chuyển vào thư mục YukiSoraShop
cd YukiSoraShop

# Chạy ứng dụng
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

## Cấu trúc Project

- **Domain**: Chứa các entities và enums
- **Application**: Chứa interfaces, models và services
- **Infrastructure**: Chứa DbContext, repositories và migrations
- **YukiSoraShop**: Ứng dụng web chính

## Troubleshooting

### Lỗi Connection String

Nếu gặp lỗi kết nối database, kiểm tra:

1. SQL Server đã được cài đặt và chạy
2. Connection string đúng với thông tin SQL Server
3. Database đã được tạo thành công

### Lỗi Migration

Nếu gặp lỗi migration:

1. Kiểm tra connection string
2. Đảm bảo SQL Server đang chạy
3. Kiểm tra quyền truy cập database

```bash
# Xóa migration và tạo lại (nếu cần)
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Tính năng

- Đăng ký/Đăng nhập người dùng
- Quản lý sản phẩm
- Giỏ hàng
- Thanh toán
- Quản lý đơn hàng
