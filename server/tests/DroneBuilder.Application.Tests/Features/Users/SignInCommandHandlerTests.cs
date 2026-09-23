using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Users.SignIn;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
namespace DroneBuilder.Application.Tests.Features.Users;

public class SignInCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
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
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockSignInManager = new Mock<SignInManager<User>>(
            _mockUserManager.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            null!, null!, null!, null!);

        _mockJwtService = new Mock<IJwtService>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockOutboxService = new Mock<IOutboxEventService>();

        _queuesConfig = new MessageQueuesConfiguration
        {
            UserQueue = new QueueConfiguration { Name = UserQueueName }
        };

        _handler = new SignInCommandHandler(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockJwtService.Object,
            _mockUserRepository.Object,
            _mockOutboxService.Object,
            _queuesConfig);
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

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, ValidPassword, true))
            .ReturnsAsync(SignInResult.Success);

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
        Result<AuthUserModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        _mockUserManager.Verify(
            x => x.FindByEmailAsync(ValidEmail),
            Times.Once);
        _mockSignInManager.Verify(
            x => x.CheckPasswordSignInAsync(user, ValidPassword, true),
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
    public async Task ExecuteCommandAsync_WhenUserNotFound_ShouldReturnFailedResultWithUnauthorizedError()
    {
        // Arrange
        var command = new SignInCommand(NotExistingEmail, ValidPassword);

        _mockUserManager
            .Setup(x => x.FindByEmailAsync(NotExistingEmail))
            .ReturnsAsync((User)null!);

        // Act & Assert
        Result<AuthUserModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<UnauthorizedError>());

        Assert.Equal("Invalid email or password.", result.Errors[0].Message);

        _mockSignInManager.Verify(
            x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()),
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
    public async Task ExecuteCommandAsync_WhenPasswordIsIncorrect_ShouldReturnFailedResultWithUnauthorizedError()
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

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, InvalidPassword, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        Result<AuthUserModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<UnauthorizedError>());

        Assert.Equal("Invalid email or password.", result.Errors[0].Message);

        _mockUserManager.Verify(
            x => x.FindByEmailAsync(ValidEmail),
            Times.Once);
        _mockSignInManager.Verify(
            x => x.CheckPasswordSignInAsync(user, InvalidPassword, true),
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

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, ValidPassword, true))
            .ReturnsAsync(SignInResult.Success);

        _mockJwtService
            .Setup(x => x.GenerateJwtTokenAsync(user.Id.ToString()))
            .ReturnsAsync(ValidToken);

        UserSignedInEvent? capturedEvent = null;
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

    [Fact]
    public async Task ExecuteCommandAsync_WhenAccountIsLockedOut_ShouldReturnFailedResultWithForbiddenError()
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

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, ValidPassword, true))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act & Assert
        Result<AuthUserModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ForbiddenError>());

        _mockJwtService.Verify(
            x => x.GenerateJwtTokenAsync(It.IsAny<string>()),
            Times.Never);
        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmailIsNotConfirmed_ShouldReturnFailedResultWithForbiddenError()
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

        _mockSignInManager
            .Setup(x => x.CheckPasswordSignInAsync(user, ValidPassword, true))
            .ReturnsAsync(SignInResult.NotAllowed);

        // Act & Assert
        Result<AuthUserModel> result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ForbiddenError>());

        Assert.Equal("Please confirm your email address before signing in.", result.Errors[0].Message);

        _mockJwtService.Verify(
            x => x.GenerateJwtTokenAsync(It.IsAny<string>()),
            Times.Never);
        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

