using Application;
using Application.DTOs;
using Application.IRepository;
using Application.Services;
using Domain.Entities;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<ICartItemRepository> _cartItemRepo = new();

    public CartServiceTests()
    {
        _uow.SetupGet(u => u.CartRepository).Returns(_cartRepo.Object);
        _uow.SetupGet(u => u.CartItemRepository).Returns(_cartItemRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }

    [Fact]
    public void IncreaseQuantity_IncrementsExistingItem()
    {
        var cartService = new CartService(_uow.Object);
        var items = new List<CartItemDTO>
        {
            new()
            {
                Product = new ProductDTO { Id = 1, Price = 10m },
                Quantity = 2
            }
        };

        var result = cartService.IncreaseQuantity(items, 1);

        Assert.Single(result);
        Assert.Equal(3, result[0].Quantity);
    }

    [Fact]
    public void DecreaseQuantity_DoesNotDropBelowOne()
    {
        var cartService = new CartService(_uow.Object);
        var items = new List<CartItemDTO>
        {
            new()
            {
                Product = new ProductDTO { Id = 1, Price = 10m },
                Quantity = 1
            }
        };

        var result = cartService.DecreaseQuantity(items, 1);

        Assert.Single(result);
        Assert.Equal(1, result[0].Quantity);
    }

    [Fact]
    public async Task AddItemAsync_AddsNewCartItem_WhenNotExists()
    {
        // Arrange
        var cart = new Cart
        {
            Id = 42,
            AccountId = 5,
            CartItems = new List<CartItem>()
        };

        _cartRepo
            .Setup(r => r.FindOneAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Cart, bool>>>(), "CartItems,CartItems.Product"))
            .ReturnsAsync(cart);

        var cartService = new CartService(_uow.Object);

        // Act
        await cartService.AddItemAsync(5, 100, 2);

        // Assert
        _cartItemRepo.Verify(r => r.AddAsync(It.Is<CartItem>(ci =>
            ci.CartId == cart.Id &&
            ci.ProductId == 100 &&
            ci.Quantity == 2)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public void GetTotal_SumsLineItems()
    {
        var cartService = new CartService(_uow.Object);
        var items = new List<CartItemDTO>
        {
            new() { Product = new ProductDTO { Id = 1, Price = 10m }, Quantity = 2 },
            new() { Product = new ProductDTO { Id = 2, Price = 5m }, Quantity = 3 }
        };

        var total = cartService.GetTotal(items);

        Assert.Equal(10m * 2 + 5m * 3, total);
    }
}
