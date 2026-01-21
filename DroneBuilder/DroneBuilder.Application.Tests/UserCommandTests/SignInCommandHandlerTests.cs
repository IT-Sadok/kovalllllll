using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Exceptions;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DroneBuilder.Application.Tests.UserCommandTests;

public class SignInCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOutboxEventService> _mockOutboxService;
    private readonly MessageQueuesConfiguration _queuesConfig;
    private readonly SignInCommandHandler _handler;

    private const string UserQueueName = "user-queue";
    private const string ValidEmail = "test@example.com";
    private const string NotExistingEmail = "notexisting@example.com";
    private const string ValidPassword = "Password123!";
    private const string InvalidPassword = "WrongPassword";
    private const string ValidToken = "ValidToken";

    public SignInCommandHandlerTests()
    {
        // Arrange
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        _mockJwtService = new Mock<IJwtService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOutboxService = new Mock<IOutboxEventService>();

        _queuesConfig = new MessageQueuesConfiguration
        {
            UserQueue = new QueueConfiguration { Name = UserQueueName }
        };

        var config = new TypeAdapterConfig();
        config.NewConfig<string, AuthUserModel>()
            .MapWith(token => new AuthUserModel { AccessToken = token });

        IMapper mapper = new Mapper(config);

        _handler = new SignInCommandHandler(
            _mockUserManager.Object,
            _mockJwtService.Object,
            _mockUserRepository.Object,
            _mockOutboxService.Object,
            _queuesConfig,
            mapper);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCredentialsAreValid_ShouldReturnAuthUserModel()
    {
        // Arrange
        var command = new SignInCommand(ValidEmail, ValidPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = ValidEmail,
            UserName = ValidEmail
        };

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(ValidEmail))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.CheckPasswordAsync(user, ValidPassword))
            .ReturnsAsync(true);

        _mockJwtService
            .Setup(x => x.GenerateJwtTokenAsync(user.Id.ToString()))
            .ReturnsAsync(ValidToken);

        _mockOutboxService
            .Setup(x => x.StoreEventAsync(
                It.Is<UserSignedInEvent>(e => e.UserId == user.Id && e.Email == ValidEmail),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUserRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockUserManager.Verify(
            x => x.FindByEmailAsync(ValidEmail),
            Times.Once);
        _mockUserManager.Verify(
            x => x.CheckPasswordAsync(user, ValidPassword),
            Times.Once);
        _mockJwtService.Verify(
            x => x.GenerateJwtTokenAsync(user.Id.ToString()),
            Times.Once);
        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedInEvent>(e => e.UserId == user.Id && e.Email == ValidEmail),
                _queuesConfig.UserQueue.Name,
                It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserNotFound_ShouldThrowInvalidEmailOrPasswordException()
    {
        // Arrange
        var command = new SignInCommand(NotExistingEmail, ValidPassword);

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(NotExistingEmail))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidEmailOrPasswordException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);

        _mockUserManager.Verify(
            x => x.CheckPasswordAsync(It.Is<User>(u => u.Email == NotExistingEmail),
                It.Is<string>(p => p == ValidPassword)),
            Times.Never);
        _mockJwtService.Verify(
            x => x.GenerateJwtTokenAsync(It.Is<string>(t => t == ValidToken)),
            Times.Never);
        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedInEvent>(e => e.UserId == Guid.Empty && e.Email == NotExistingEmail),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenPasswordIsIncorrect_ShouldThrowInvalidEmailOrPasswordException()
    {
        // Arrange
        var command = new SignInCommand(ValidEmail, InvalidPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = ValidEmail,
            UserName = ValidEmail
        };

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(ValidEmail))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.CheckPasswordAsync(user, InvalidPassword))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidEmailOrPasswordException>(() =>
            _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);

        _mockUserManager.Verify(
            x => x.FindByEmailAsync(ValidEmail),
            Times.Once);
        _mockUserManager.Verify(
            x => x.CheckPasswordAsync(user, InvalidPassword),
            Times.Once);
        _mockJwtService.Verify(
            x => x.GenerateJwtTokenAsync(It.Is<string>(t => t == ValidToken)),
            Times.Never);
        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedInEvent>(e => e.UserId == user.Id && e.Email == ValidEmail),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenSuccessful_ShouldGenerateCorrectEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SignInCommand(ValidEmail, ValidPassword);

        var user = new User
        {
            Id = userId,
            Email = ValidEmail,
            UserName = ValidEmail
        };

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(ValidEmail))
            .ReturnsAsync(user);

        _mockUserManager
            .Setup(x => x.CheckPasswordAsync(user, ValidPassword))
            .ReturnsAsync(true);

        _mockJwtService
            .Setup(x => x.GenerateJwtTokenAsync(user.Id.ToString()))
            .ReturnsAsync(ValidToken);

        UserSignedInEvent capturedEvent = null;
        _mockOutboxService
            .Setup(x => x.StoreEventAsync(
                It.Is<UserSignedInEvent>(e => e.UserId == userId && e.Email == ValidEmail),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()))
            .Callback((UserSignedInEvent e, string _, CancellationToken _) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedEvent);
        Assert.Equal(userId, capturedEvent.UserId);
        Assert.Equal(ValidEmail, capturedEvent.Email);
    }
}