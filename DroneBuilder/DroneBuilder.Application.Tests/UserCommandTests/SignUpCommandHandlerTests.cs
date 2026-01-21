using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Models.UserModels;
using DroneBuilder.Application.Options;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DroneBuilder.Application.Tests.UserCommandTests;

public class SignUpCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOutboxEventService> _mockOutboxService;
    private readonly MessageQueuesConfiguration _queuesConfig;
    private readonly SignUpCommandHandler _handler;

    private const string UserQueueName = "user-queue";
    private const string ValidEmail = "test@example.com";
    private const string ValidPassword = "Password123!";
    private const string InvalidPassword = "weak";
    private const string ErrorMessage = "Password too weak";

    public SignUpCommandHandlerTests()
    {
        // Arrange
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        _mockUserRepository = new Mock<IUserRepository>();
        _mockOutboxService = new Mock<IOutboxEventService>();

        _queuesConfig = new MessageQueuesConfiguration
        {
            UserQueue = new QueueConfiguration { Name = UserQueueName }
        };

        _handler = new SignUpCommandHandler(
            _mockUserManager.Object,
            _mockUserRepository.Object,
            _mockOutboxService.Object,
            _queuesConfig);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserCreationSucceeds_ShouldStoreEventAndSaveChanges()
    {
        // Arrange
        var signUpModel = new SignUpModel
        {
            Email = ValidEmail,
            Password = ValidPassword
        };
        var command = new SignUpUserCommand(signUpModel);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == signUpModel.Email),
                It.Is<string>(p => p == signUpModel.Password)))
            .ReturnsAsync(IdentityResult.Success);

        _mockOutboxService
            .Setup(x => x.StoreEventAsync(
                It.Is<UserSignedUpEvent>(e => e.Email == signUpModel.Email),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUserRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _mockUserManager.Verify(
            x => x.CreateAsync(It.Is<User>(u => u.Email == signUpModel.Email), signUpModel.Password),
            Times.Once);

        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedUpEvent>(e => e.Email == signUpModel.Email),
                _queuesConfig.UserQueue.Name,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserCreationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var signUpModel = new SignUpModel
        {
            Email = ValidEmail,
            Password = InvalidPassword
        };
        var command = new SignUpUserCommand(signUpModel);

        var errors = new[]
        {
            new IdentityError { Description = ErrorMessage }
        };

        var failedResult = IdentityResult.Failed(errors);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == signUpModel.Email),
                It.Is<string>(p => p == signUpModel.Password)))
            .ReturnsAsync(failedResult);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.ExecuteCommandAsync(command, CancellationToken.None));

        Assert.Contains("User creation failed", exception.Message);
        Assert.Contains("Password too weak", exception.Message);

        // Assert
        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedUpEvent>(e => e.Email == signUpModel.Email),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserHasNoEmail_ShouldNotStoreEvent()
    {
        // Arrange
        var signUpModel = new SignUpModel
        {
            Email = null,
            Password = ValidPassword
        };
        var command = new SignUpUserCommand(signUpModel);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == null),
                It.Is<string>(p => p == signUpModel.Password)))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.Is<UserSignedUpEvent>(e => e.Email == null),
                It.Is<string>(q => q == _queuesConfig.UserQueue.Name),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}