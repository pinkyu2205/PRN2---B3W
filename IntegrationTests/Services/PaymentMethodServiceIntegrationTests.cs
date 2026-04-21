using System;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Application.IRepository;
using Application.Services;
using Application.Services.Interfaces;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntegrationTests.Services;

public class PaymentMethodServiceIntegrationTests
{
    [Fact]
    public async Task SetStatusAsync_TogglesMethodAndPersistsChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PaymentMethods-{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        await SeedAsync(context);

        var paymentMethodRepository = new PaymentMethodRepository(context);
        var uow = new TestUnitOfWork(context, paymentMethodRepository);
        IPaymentMethodService service = new PaymentMethodService(uow);

        // Act
        var result = await service.SetStatusAsync(1, isActive: false, modifiedBy: "admin", CancellationToken.None);
        await context.Entry(await context.PaymentMethods.FindAsync(1)).ReloadAsync();

        // Assert
        Assert.True(result);
        var method = await context.PaymentMethods.FirstAsync(pm => pm.Id == 1);
        Assert.False(method.IsActive);
        Assert.Equal("admin", method.ModifiedBy);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveMethods()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OnlyActive-{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        await SeedAsync(context);

        var paymentMethodRepository = new PaymentMethodRepository(context);
        var uow = new TestUnitOfWork(context, paymentMethodRepository);
        IPaymentMethodService service = new PaymentMethodService(uow);

        await service.SetStatusAsync(2, false, "admin");

        var active = await service.GetActiveAsync();

        Assert.Single(active);
        Assert.Equal("VNPay", active[0].Name);
    }

    [Fact]
    public async Task SetStatusAsync_ReactivatesInactiveMethod()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Reactivate-{Guid.NewGuid()}")
            .Options;

        var context = new AppDbContext(options);
        await SeedAsync(context);

        var paymentMethodRepository = new PaymentMethodRepository(context);
        var uow = new TestUnitOfWork(context, paymentMethodRepository);
        IPaymentMethodService service = new PaymentMethodService(uow);

        await service.SetStatusAsync(1, false, "admin-off");

        var result = await service.SetStatusAsync(1, true, "admin-on");
        await context.Entry(await context.PaymentMethods.FindAsync(1)).ReloadAsync();

        Assert.True(result);
        var method = await context.PaymentMethods.FirstAsync(pm => pm.Id == 1);
        Assert.True(method.IsActive);
        Assert.Equal("admin-on", method.ModifiedBy);
    }

    private static async Task SeedAsync(AppDbContext context)
    {
        context.PaymentMethods.AddRange(
            new PaymentMethod
            {
                Id = 1,
                Name = "VNPay",
                Description = "Gateway",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            },
            new PaymentMethod
            {
                Id = 2,
                Name = "Cash",
                Description = "COD",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public TestUnitOfWork(AppDbContext context, IPaymentMethodRepository paymentMethodRepository)
        {
            _context = context;
            PaymentMethodRepository = paymentMethodRepository;
        }

        public IAccountRepository AccountRepository => throw new NotImplementedException();
        public ICartItemRepository CartItemRepository => throw new NotImplementedException();
        public ICartRepository CartRepository => throw new NotImplementedException();
        public IInvoiceDetailRepository InvoiceDetailRepository => throw new NotImplementedException();
        public IInvoiceRepository InvoiceRepository => throw new NotImplementedException();
        public IOrderDetailRepository OrderDetailRepository => throw new NotImplementedException();
        public IOrderRepository OrderRepository => throw new NotImplementedException();
        public IPaymentMethodRepository PaymentMethodRepository { get; }
        public IPaymentRepository PaymentRepository => throw new NotImplementedException();
        public IProductRepository ProductRepository => throw new NotImplementedException();
        public ICategoryRepository CategoryRepository => throw new NotImplementedException();
        public IProductDetailRepository ProductDetailRepository => throw new NotImplementedException();

        //public IProductDetailRepository ProductDetailRepository => throw new NotImplementedException();

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction() =>
            throw new NotImplementedException();

        public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync() =>
            throw new NotImplementedException();
    }
}
