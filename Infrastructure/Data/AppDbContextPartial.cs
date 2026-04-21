using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configure Category-Product relationship
        modelBuilder.Entity<Product>(entity =>
        {
            // Primary foreign key relationship using CategoryId
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Categories_CategoryId");
                
            // Configure CategoryName as display field
            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(255);
                
            // Create index for CategoryName for better search performance
            entity.HasIndex(e => e.CategoryName)
                .HasDatabaseName("IX_Products_CategoryName");
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Categories");
            
            entity.HasIndex(e => e.CategoryName)
                .IsUnique()
                .HasDatabaseName("IX_Categories_CategoryName_Unique");
            
            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Roles");
            
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.HasIndex(e => e.RoleName)
                .IsUnique()
                .HasDatabaseName("IX_Roles_RoleName_Unique");

            // Seed default roles
            entity.HasData(
                new 
                { 
                    Id = 1, 
                    RoleName = "Customer", 
                    Description = "Khách hàng", 
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                },
                new 
                { 
                    Id = 2, 
                    RoleName = "Administrator", 
                    Description = "Quản trị viên", 
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                },
                new 
                { 
                    Id = 3, 
                    RoleName = "Moderator", 
                    Description = "Điều hành viên", 
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                }
            );
        });

        // Configure Account-Role relationship
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(d => d.Role)
                .WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Accounts_Roles_RoleId");

            entity.Property(e => e.RoleId)
                .HasDefaultValue(1); // Default to Customer role

            // Seed default accounts
            entity.HasData(
                new 
                { 
                    Id = 1, 
                    UserName = "admin@gmail.com",
                    FullName = "Administrator",
                    Email = "admin@gmail.com",
                    Password = "admin123",
                    PhoneNumber = "0123456789",
                    Address = "",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "",
                    AvatarUrl = "",
                    RoleId = 2, // Administrator role
                    Status = "Active",
                    IsExternal = false,
                    ExternalProvider = (string)null,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                },
                new 
                { 
                    Id = 2, 
                    UserName = "staff@gmail.com",
                    FullName = "Staff Member",
                    Email = "staff@gmail.com",
                    Password = "staff123",
                    PhoneNumber = "0123456788",
                    Address = "",
                    DateOfBirth = new DateTime(1992, 1, 1),
                    Gender = "",
                    AvatarUrl = "",
                    RoleId = 3, // Moderator role
                    Status = "Active",
                    IsExternal = false,
                    ExternalProvider = (string)null,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                },
                new 
                { 
                    Id = 3, 
                    UserName = "customer@gmail.com",
                    FullName = "Customer User",
                    Email = "customer@gmail.com",
                    Password = "customer123",
                    PhoneNumber = "0123456787",
                    Address = "",
                    DateOfBirth = new DateTime(1995, 1, 1),
                    Gender = "",
                    AvatarUrl = "",
                    RoleId = 1, // Customer role
                    Status = "Active",
                    IsExternal = false,
                    ExternalProvider = (string)null,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System", 
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                }
            );
        });

        // Configure ProductDetail entity
        modelBuilder.Entity<ProductDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("ProductDetails");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.ProductDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ProductDetails_Products_ProductId");

            entity.Property(e => e.Color)
                .HasMaxLength(50);

            entity.Property(e => e.Size)
                .HasMaxLength(20);

            entity.Property(e => e.Material)
                .HasMaxLength(100);

            entity.Property(e => e.Origin)
                .HasMaxLength(100);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
        });

        // Configure payment methods and seed defaults
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("PaymentMethods");

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.HasData(
                new
                {
                    Id = 1,
                    Name = "VNPay",
                    Description = "VNPay payment gateway",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System",
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                },
                new
                {
                    Id = 2,
                    Name = "Cash",
                    Description = "Thanh toán tiền mặt",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "System",
                    ModifiedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    ModifiedBy = "System",
                    IsDeleted = false
                }
            );
        });

        // Configure Shop entity
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Shops");

            entity.Property(e => e.ShopName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            // 1-1 relationship: Account <-> Shop
            entity.HasOne(s => s.Owner)
                .WithOne(a => a.Shop)
                .HasForeignKey<Shop>(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Shops_Accounts_OwnerId");

            entity.HasIndex(e => e.OwnerId)
                .IsUnique()
                .HasDatabaseName("IX_Shops_OwnerId_Unique");

            entity.HasIndex(e => e.ShopName)
                .HasDatabaseName("IX_Shops_ShopName");
        });

        // Configure ShopApplication entity
        modelBuilder.Entity<ShopApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("ShopApplications");

            entity.Property(e => e.ShopName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.AdminNote)
                .HasMaxLength(1000);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(sa => sa.Applicant)
                .WithMany(a => a.ShopApplications)
                .HasForeignKey(sa => sa.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ShopApplications_Accounts_ApplicantId");

            entity.HasIndex(e => e.ApplicantId)
                .HasDatabaseName("IX_ShopApplications_ApplicantId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_ShopApplications_Status");
        });

        // Configure Product-Shop relationship (nullable FK)
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Shop)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Products_Shops_ShopId")
                .IsRequired(false);

            entity.HasIndex(e => e.ShopId)
                .HasDatabaseName("IX_Products_ShopId");
        });
    }
}
