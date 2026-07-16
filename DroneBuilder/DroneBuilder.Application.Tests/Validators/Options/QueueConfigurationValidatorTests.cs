using DroneBuilder.Application.Options;
using DroneBuilder.Application.Validation.Validators.Options;
using FluentValidation.TestHelper;
using Xunit;

namespace DroneBuilder.Application.Tests.Validators.Options;

public class QueueConfigurationValidatorTests
{
    private readonly QueueConfigurationValidator _validator;

    public QueueConfigurationValidatorTests()
    {
        _validator = new QueueConfigurationValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var model = new QueueConfiguration { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Queue name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_MaxRetryCount_Is_Negative()
    {
        var model = new QueueConfiguration { Name = "test", MaxRetryCount = -1 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MaxRetryCount);
    }

    [Fact]
    public void Should_Have_Error_When_PrefetchCount_Is_Zero_Or_Negative()
    {
        var model = new QueueConfiguration { Name = "test", PrefetchCount = 0 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PrefetchCount);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Configuration_Is_Valid()
    {
        var model = new QueueConfiguration
        {
            Name = "ValidQueue",
            MaxRetryCount = 3,
            PrefetchCount = 10
        };

        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
