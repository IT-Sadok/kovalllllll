using System.Text;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Users.ConfirmEmail;
using DroneBuilder.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
namespace DroneBuilder.Application.Tests.Features.Users;

public class ConfirmEmailCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly ConfirmEmailCommandHandler _handler;

    private static readonly Guid UserId = Guid.NewGuid();

    private const string RawToken = "raw-identity-token";

    private static readonly string EncodedToken =
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(RawToken));

    public ConfirmEmailCommandHandlerTests()
    {
        // Arrange
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new ConfirmEmailCommandHandler(_mockUserManager.Object);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTokenIsValid_ShouldConfirmEmail()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId, EncodedToken);
        var user = new User { Id = UserId, EmailConfirmed = false };

        _mockUserManager.Setup(x => x.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, RawToken)).ReturnsAsync(IdentityResult.Success);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockUserManager.Verify(x => x.ConfirmEmailAsync(user, RawToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserNotFound_ShouldReturnFailedResultWithNotFoundError()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId, EncodedToken);

        _mockUserManager.Setup(x => x.FindByIdAsync(UserId.ToString())).ReturnsAsync((User)null!);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmailAlreadyConfirmed_ShouldSucceedWithoutConfirmingAgain()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId, EncodedToken);
        var user = new User { Id = UserId, EmailConfirmed = true };

        _mockUserManager.Setup(x => x.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockUserManager.Verify(
            x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTokenIsMalformed_ShouldReturnFailedResultWithBadRequestError()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId, "not+valid+base64url!!");
        var user = new User { Id = UserId, EmailConfirmed = false };

        _mockUserManager.Setup(x => x.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Confirmation token is malformed.", result.Errors[0].Message);

        _mockUserManager.Verify(
            x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenTokenIsRejected_ShouldReturnFailedResultWithBadRequestError()
    {
        // Arrange
        var command = new ConfirmEmailCommand(UserId, EncodedToken);
        var user = new User { Id = UserId, EmailConfirmed = false };

        _mockUserManager.Setup(x => x.FindByIdAsync(UserId.ToString())).ReturnsAsync(user);
        _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, RawToken))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

        // Act & Assert
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Equal("Confirmation token is invalid or has expired.", result.Errors[0].Message);
    }
}
