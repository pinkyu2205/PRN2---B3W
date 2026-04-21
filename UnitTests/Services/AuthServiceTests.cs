using Application;
using Application.DTOs;
using Application.IRepository;
using Application.Services;
using Domain.Entities;
using Moq;
using System.Threading.Tasks;

namespace UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();

    public AuthServiceTests()
    {
        _uowMock.SetupGet(u => u.AccountRepository).Returns(_accountRepoMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsFalse_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new AccountRegistrationDTO { Email = "existing@example.com", Password = "P@ssw0rd", FullName = "User" };
        _accountRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(new Account { Email = dto.Email });

        var service = new AuthService(_uowMock.Object);

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.False(result);
        _accountRepoMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_PersistsAccount_WhenEmailIsNew()
    {
        // Arrange
        var dto = new AccountRegistrationDTO
        {
            Email = "new@example.com",
            Password = "P@ssw0rd",
            FullName = "New User",
            PhoneNumber = "0123456789"
        };

        _accountRepoMock
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((Account?)null);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new AuthService(_uowMock.Object);

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.True(result);
        _accountRepoMock.Verify(r => r.AddAsync(It.Is<Account>(a =>
            a.Email == dto.Email &&
            a.Password == dto.Password &&
            a.FullName == dto.FullName &&
            a.RoleId == 1 &&
            a.Status == "Active")), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAccount_WhenPasswordMatches()
    {
        // Arrange
        var account = new Account
        {
            Email = "user@example.com",
            Password = "secret",
            FullName = "Existing"
        };

        _accountRepoMock
            .Setup(r => r.GetByEmailAsync(account.Email))
            .ReturnsAsync(account);

        var service = new AuthService(_uowMock.Object);

        // Act
        var result = await service.LoginAsync(account.Email, "secret");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Email, result?.Email);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var account = new Account
        {
            Email = "user@example.com",
            Password = "secret",
            FullName = "Existing"
        };

        _accountRepoMock
            .Setup(r => r.GetByEmailAsync(account.Email))
            .ReturnsAsync(account);

        var service = new AuthService(_uowMock.Object);

        // Act
        var result = await service.LoginAsync(account.Email, "invalid");

        // Assert
        Assert.Null(result);
    }
}
