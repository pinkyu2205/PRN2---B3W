using Application;
using Application.DTOs;
using Application.IRepository;
using Application.Services;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Domain.Entities;
using Moq;
using UnitTests.Helpers;

namespace UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly IMapper _mapper;

    public ProductServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ProductDetail, ProductDetailDTO>();
            cfg.CreateMap<Product, ProductDTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.ProductName))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.CategoryName))
                .ForMember(d => d.Stock, o => o.MapFrom(s => s.StockQuantity));
        }, new NullLoggerFactory());
        _mapper = config.CreateMapper();

        _uow.SetupGet(u => u.ProductRepository).Returns(_productRepo.Object);
        _uow.SetupGet(u => u.CategoryRepository).Returns(_categoryRepo.Object);
    }

    //[Fact]
    //public async Task GetProductsByCategoryAsync_FiltersByCategoryName()
    //{
    //    // Arrange
    //    var products = new List<Product>
    //    {
    //        new() { Id = 1, ProductName = "Ring", CategoryName = "Jewelry", Price = 100 },
    //        new() { Id = 2, ProductName = "Necklace", CategoryName = "Jewelry", Price = 200 },
    //        new() { Id = 3, ProductName = "Wallet", CategoryName = "Accessories", Price = 50 }
    //    };

    //    var queryable = new TestAsyncEnumerable<Product>(products.Select(p =>
    //    {
    //        p.ProductDetails = new List<ProductDetail>();
    //        return p;
    //    }));
    //    _productRepo.Setup(r => r.GetAllQueryable("ProductDetails")).Returns(queryable);

    //    var service = new ProductService(_uow.Object, _mapper);

    //    // Act
    //    var result = await service.GetProductsByCategoryAsync("jewelry");

    //    // Assert
    //    Assert.Equal(2, result.Count);
    //    Assert.All(result, dto => Assert.Equal("Jewelry", dto.Category));
    //}

    [Fact]
    public async Task CreateProductAsync_ReturnsTrue_WhenRepositorySucceeds()
    {
        // Arrange
        var product = new Product { Id = 5, ProductName = "New Item", Price = 99 };
        _productRepo.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var service = new ProductService(_uow.Object, _mapper);

        // Act
        var result = await service.CreateProductAsync(product);

        // Assert
        Assert.True(result);
        _productRepo.Verify(r => r.AddAsync(product), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_SoftDeletes_WhenFound()
    {
        // Arrange
        var product = new Product { Id = 9, ProductName = "Old Product" };
        _productRepo.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var service = new ProductService(_uow.Object, _mapper);

        // Act
        var result = await service.DeleteProductAsync(product.Id);

        // Assert
        Assert.True(result);
        _productRepo.Verify(r => r.SoftDelete(product), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ReturnsFalse_WhenProductMissing()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product?)null);

        var service = new ProductService(_uow.Object, _mapper);

        var result = await service.DeleteProductAsync(123);

        Assert.False(result);
        _productRepo.Verify(r => r.SoftDelete(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
