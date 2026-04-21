using Application;
using Application.IRepository;
using Application.Services;
using Domain.Enums;
using Moq;
using System.Threading.Tasks;

namespace UnitTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();

    public PaymentServiceTests()
    {
        _uow.SetupGet(u => u.PaymentRepository).Returns(_paymentRepo.Object);
    }

    [Fact]
    public async Task GetTotalRevenueAsync_ReturnsRepositoryValue()
    {
        // Arrange
        _paymentRepo.Setup(r => r.GetTotalRevenueAsync(PaymentStatus.Paid)).ReturnsAsync(1234.56m);
        var service = new PaymentService(_uow.Object);

        // Act
        var revenue = await service.GetTotalRevenueAsync();

        // Assert
        Assert.Equal(1234.56m, revenue);
        _paymentRepo.Verify(r => r.GetTotalRevenueAsync(PaymentStatus.Paid), Times.Once);
    }
}
