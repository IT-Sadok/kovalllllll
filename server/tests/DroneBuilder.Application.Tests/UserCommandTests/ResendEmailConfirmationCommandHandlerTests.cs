using DroneBuilder.Application.Abstractions;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DroneBuilder.Application.Tests.UserCommandTests;

public class ResendEmailConfirmationCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly ResendEmailConfirmationCommandHandler _handler;

    private const string Email = "test@example.com";

    public ResendEmailConfirmationCommandHandlerTests()
    {
        // Arrange
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUserManager
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("confirmation-token");

        _mockEmailSender = new Mock<IEmailSender>();
        _mockEmailSender
            .Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new ResendEmailConfirmationCommandHandler(_mockUserManager.Object, _mockEmailSender.Object);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAccountIsUnconfirmed_ShouldSendConfirmationEmail()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = Email, EmailConfirmed = false };
        _mockUserManager.Setup(x => x.FindByEmailAsync(Email)).ReturnsAsync(user);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new ResendEmailConfirmationCommand(Email), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(Email, user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAccountIsConfirmed_ShouldSucceedWithoutSending()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByEmailAsync(Email))
            .ReturnsAsync(new User { Email = Email, EmailConfirmed = true });

        // Act
        Result result = await _handler.ExecuteCommandAsync(new ResendEmailConfirmationCommand(Email), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenAccountDoesNotExist_ShouldSucceedWithoutSending()
    {
        // Arrange
        _mockUserManager.Setup(x => x.FindByEmailAsync(Email)).ReturnsAsync((User?)null);

        // Act
        Result result = await _handler.ExecuteCommandAsync(new ResendEmailConfirmationCommand(Email), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
