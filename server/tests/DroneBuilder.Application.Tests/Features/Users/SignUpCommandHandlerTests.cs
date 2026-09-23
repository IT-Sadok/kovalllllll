using DroneBuilder.Application.Common.Abstractions;
using DroneBuilder.Application.Common.Options;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Common.ResultErrors;
using DroneBuilder.Application.Features.Users.SignUp;
using DroneBuilder.Domain.Constants;
using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Events.UserEvents;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Moq;
namespace DroneBuilder.Application.Tests.Features.Users;

public class SignUpCommandHandlerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IOutboxEventService> _mockOutboxService;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly MessageQueuesConfiguration _queuesConfig;
    private readonly SignUpCommandHandler _handler;

    private const string UserQueueName = "user-queue";
    private const string ValidEmail = "test@example.com";
    private const string ValidPassword = "Password123!";
    private const string InvalidPassword = "weak";
    private const string ErrorMessage = "Password too weak";
    private const string ConfirmationToken = "confirmation-token";

    public SignUpCommandHandlerTests()
    {
        // Arrange
        var userStore = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(ConfirmationToken);

        _mockUserRepository = new Mock<IUserRepository>();
        _mockOutboxService = new Mock<IOutboxEventService>();

        _mockEmailSender = new Mock<IEmailSender>();
        _mockEmailSender
            .Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _queuesConfig = new MessageQueuesConfiguration
        {
            UserQueue = new QueueConfiguration { Name = UserQueueName }
        };

        _handler = new SignUpCommandHandler(
            _mockUserManager.Object,
            _mockUserRepository.Object,
            _mockOutboxService.Object,
            _mockEmailSender.Object,
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

        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(
                signUpModel.Email, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserManager.Verify(
            x => x.AddToRoleAsync(It.Is<User>(u => u.Email == signUpModel.Email), RoleNames.User),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenRoleAssignmentFails_ShouldDeleteUserAndFail()
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

        _mockUserManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role does not exist." }));

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Contains("Could not assign the default role", result.Errors[0].Message);

        _mockUserManager.Verify(
            x => x.DeleteAsync(It.Is<User>(u => u.Email == signUpModel.Email)),
            Times.Once);

        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenConfirmationEmailFails_ShouldDeleteUserAndFail()
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

        _mockEmailSender
            .Setup(x => x.SendEmailConfirmationAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new BadRequestError("Could not send the confirmation email.")));

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        _mockUserManager.Verify(
            x => x.DeleteAsync(It.Is<User>(u => u.Email == signUpModel.Email)),
            Times.Once);

        _mockOutboxService.Verify(
            x => x.StoreEventAsync(
                It.IsAny<UserSignedUpEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUserRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUserCreationFails_ShouldReturnFailedResultWithBadRequestError()
    {
        // Arrange
        var signUpModel = new SignUpModel
        {
            Email = ValidEmail,
            Password = InvalidPassword
        };
        var command = new SignUpUserCommand(signUpModel);

        IdentityError[] errors = new[]
        {
            new IdentityError { Description = ErrorMessage }
        };

        var failedResult = IdentityResult.Failed(errors);

        _mockUserManager
            .Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == signUpModel.Email),
                It.Is<string>(p => p == signUpModel.Password)))
            .ReturnsAsync(failedResult);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());

        Assert.Contains("User creation failed", result.Errors[0].Message);
        Assert.Contains("Password too weak", result.Errors[0].Message);

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
            Email = null!,
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

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmailBelongsToUnconfirmedAccount_ShouldResendConfirmationAndSucceed()
    {
        // Arrange
        var command = new SignUpUserCommand(new SignUpModel { Email = ValidEmail, Password = ValidPassword });
        var existingUser = new User { Id = Guid.NewGuid(), Email = ValidEmail, EmailConfirmed = false };

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(DuplicateEmailError()));
        _mockUserManager
            .Setup(x => x.FindByEmailAsync(ValidEmail))
            .ReturnsAsync(existingUser);

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(ValidEmail, existingUser.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _mockUserRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmailBelongsToConfirmedAccount_ShouldSucceedWithoutSendingEmail()
    {
        // Arrange
        var command = new SignUpUserCommand(new SignUpModel { Email = ValidEmail, Password = ValidPassword });

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(DuplicateEmailError()));
        _mockUserManager
            .Setup(x => x.FindByEmailAsync(ValidEmail))
            .ReturnsAsync(new User { Email = ValidEmail, EmailConfirmed = true });

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockEmailSender.Verify(
            x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenEmailTakenAndPasswordWeak_ShouldReportOnlyPasswordErrors()
    {
        // Arrange
        var command = new SignUpUserCommand(new SignUpModel { Email = ValidEmail, Password = InvalidPassword });

        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                DuplicateEmailError(),
                new IdentityError { Code = "PasswordTooShort", Description = ErrorMessage }));

        // Act
        Result result = await _handler.ExecuteCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.True(result.HasError<BadRequestError>());
        Assert.Contains(ErrorMessage, result.Errors[0].Message);
        Assert.DoesNotContain("already taken", result.Errors[0].Message);
    }

    private static IdentityError DuplicateEmailError() => new()
    {
        Code = nameof(IdentityErrorDescriber.DuplicateEmail),
        Description = $"Email '{ValidEmail}' is already taken."
    };
}
