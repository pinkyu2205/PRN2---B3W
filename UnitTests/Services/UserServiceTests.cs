using Application;
using Application.DTOs;
using Application.IRepository;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IAccountRepository> _accountRepo = new();
    private readonly IMapper _mapper;

    public UserServiceTests()
    {
        var config = new MapperConfiguration(expression =>
        {
            expression.CreateMap<Account, AccountDTO>();
        }, new NullLoggerFactory()); ;
        _mapper = config.CreateMapper();

        _uow.SetupGet(u => u.AccountRepository).Returns(_accountRepo.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsDto_WhenAccountExists()
    {
        var account = new Account
        {
            Id = 10,
            FullName = "Test User",
            Email = "user@example.com"
        };

        _accountRepo.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);

        var service = new UserService(_uow.Object, _mapper);

        var result = await service.GetUserByIdAsync(account.Id);

        Assert.NotNull(result);
        Assert.Equal(account.FullName, result?.FullName);
    }

    [Fact]
    public async Task UpdateProfileAsync_ReturnsFalse_WhenAccountMissing()
    {
        _accountRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Account?)null);
        var service = new UserService(_uow.Object, _mapper);
        var command = new UpdateProfileCommand { AccountId = 42 };

        var result = await service.UpdateProfileAsync(command);

        Assert.False(result);
        _accountRepo.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_TrimsValuesAndPersists()
    {
        var account = new Account
        {
            Id = 5,
            FullName = "Old Name",
            Email = "user@example.com",
            Address = "Old address",
            Gender = "",
            AvatarUrl = ""
        };

        _accountRepo.Setup(r => r.GetByIdAsync(account.Id)).ReturnsAsync(account);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var service = new UserService(_uow.Object, _mapper);
        var command = new UpdateProfileCommand
        {
            AccountId = account.Id,
            FullName = "  New Name  ",
            PhoneNumber = " 0123 ",
            Address = " 123 Street  ",
            Gender = " Male ",
            AvatarUrl = " https://img ",
            ModifiedBy = " admin ",
            DateOfBirth = new DateTime(1995, 5, 1)
        };

        var result = await service.UpdateProfileAsync(command);

        Assert.True(result);
        Assert.Equal("New Name", account.FullName);
        Assert.Equal("0123", account.PhoneNumber);
        Assert.Equal("123 Street", account.Address);
        Assert.Equal("Male", account.Gender);
        Assert.Equal("https://img", account.AvatarUrl);
        Assert.Equal(new DateTime(1995, 5, 1), account.DateOfBirth);
        Assert.Equal(command.ModifiedBy, account.ModifiedBy);
        _accountRepo.Verify(r => r.Update(account), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
