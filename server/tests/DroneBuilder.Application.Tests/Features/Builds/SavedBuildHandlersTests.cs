using DroneBuilder.Application.Common.Contexts;
using DroneBuilder.Application.Common.Errors;
using DroneBuilder.Application.Common.Repositories;
using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds;
using DroneBuilder.Application.Features.Builds.SavedBuilds.CreateBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds.DeleteBuild;
using DroneBuilder.Application.Features.Builds.SavedBuilds.GetBuildById;
using DroneBuilder.Application.Features.Builds.SavedBuilds.UpdateBuild;
using DroneBuilder.Domain.Entities;
using FluentResults;
using FluentValidation.Results;
using NSubstitute;

namespace DroneBuilder.Application.Tests.Features.Builds;

public class SavedBuildHandlersTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IBuildRepository _buildRepository = Substitute.For<IBuildRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    private readonly Product _frame = new() { Name = "Frame", Category = ProductCategory.Frame, Price = 60m };
    private readonly Product _motor = new() { Name = "Motor", Category = ProductCategory.Motor, Price = 20m };
    private readonly Product _battery = new() { Name = "Battery", Category = ProductCategory.Battery, Price = 50m };

    public SavedBuildHandlersTests()
    {
        _userContext.UserId.Returns(UserId);
        _productRepository.GetProductsByIdsAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call => new[] { _frame, _motor, _battery }
                .Where(p => call.Arg<ICollection<Guid>>().Contains(p.Id)).ToList());
    }

    private void ReturnBuildWithProducts(Build build)
    {
        _buildRepository.GetUserBuildAsync(build.Id, UserId, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                Dictionary<Guid, Product> products = new[] { _frame, _motor, _battery }.ToDictionary(p => p.Id);
                foreach (BuildItem item in build.Items)
                {
                    item.Product = products[item.ProductId];
                }

                return build;
            });
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCreatingBuild_ShouldSaveItForCurrentUser()
    {
        // Arrange
        Build? added = null;
        await _buildRepository.AddAsync(Arg.Do<Build>(b =>
        {
            added = b;
            ReturnBuildWithProducts(b);
        }), Arg.Any<CancellationToken>());
        var handler = new CreateBuildCommandHandler(_buildRepository, _productRepository, _userContext);

        // Act
        Result<SavedBuildModel> result = await handler.ExecuteCommandAsync(
            new CreateBuildCommand(new SaveBuildModel("  My 5 inch  ",
                [new BuildItemModel(_frame.Id, 1), new BuildItemModel(_motor.Id, 4)])),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(UserId, added.UserId);
        Assert.Equal("My 5 inch", added.Name);
        Assert.Equal(140m, result.Value.TotalPrice);
        Assert.Equal(2, result.Value.Items.Count);
        await _buildRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenCreatingBuildWithUnknownProduct_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new CreateBuildCommandHandler(_buildRepository, _productRepository, _userContext);

        // Act
        Result<SavedBuildModel> result = await handler.ExecuteCommandAsync(
            new CreateBuildCommand(new SaveBuildModel("Build", [new BuildItemModel(Guid.NewGuid(), 1)])),
            CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
        await _buildRepository.DidNotReceive().AddAsync(Arg.Any<Build>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenUpdatingBuild_ShouldKeepMatchingItemsAndReplaceTheRest()
    {
        // Arrange
        var frameItem = new BuildItem { ProductId = _frame.Id, Quantity = 1 };
        var motorItem = new BuildItem { ProductId = _motor.Id, Quantity = 4 };
        var build = new Build { UserId = UserId, Name = "Old", Items = [frameItem, motorItem] };
        DateTime previousUpdate = build.UpdatedAt;
        ReturnBuildWithProducts(build);
        var handler = new UpdateBuildCommandHandler(_buildRepository, _productRepository, _userContext);

        // Act
        Result<SavedBuildModel> result = await handler.ExecuteCommandAsync(
            new UpdateBuildCommand(build.Id, new SaveBuildModel("New",
                [new BuildItemModel(_frame.Id, 1), new BuildItemModel(_battery.Id, 3)])),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New", build.Name);
        Assert.True(build.UpdatedAt >= previousUpdate);
        Assert.Contains(frameItem, build.Items);
        Assert.DoesNotContain(motorItem, build.Items);
        Assert.Equal(3, build.Items.Single(i => i.ProductId == _battery.Id).Quantity);
        await _buildRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenBuildBelongsToAnotherUser_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new UpdateBuildCommandHandler(_buildRepository, _productRepository, _userContext);

        // Act
        Result<SavedBuildModel> result = await handler.ExecuteCommandAsync(
            new UpdateBuildCommand(Guid.NewGuid(), new SaveBuildModel("Build", [new BuildItemModel(_frame.Id, 1)])),
            CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
        await _buildRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteCommandAsync_WhenDeletingOwnBuild_ShouldRemoveIt()
    {
        // Arrange
        var build = new Build { UserId = UserId, Name = "Build" };
        ReturnBuildWithProducts(build);
        var handler = new DeleteBuildCommandHandler(_buildRepository, _userContext);

        // Act
        Result result = await handler.ExecuteCommandAsync(new DeleteBuildCommand(build.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _buildRepository.Received(1).Remove(build);
        await _buildRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuildIsMissing_ShouldReturnNotFound()
    {
        // Arrange
        var handler = new GetBuildByIdQueryHandler(_buildRepository, _userContext);

        // Act
        Result<SavedBuildModel> result = await handler.ExecuteAsync(new GetBuildByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public void ToModel_WhenAProductWasDelisted_ShouldMarkItUnavailableAndLeaveItOutOfTheTotal()
    {
        // Arrange
        _motor.IsDeleted = true;
        var build = new Build
        {
            Name = "Build",
            Items =
            [
                new BuildItem { ProductId = _motor.Id, Product = _motor, Quantity = 4 },
                new BuildItem { ProductId = _frame.Id, Product = _frame, Quantity = 1 }
            ]
        };

        // Act
        SavedBuildModel model = build.ToModel();

        // Assert
        Assert.Equal(60m, model.TotalPrice);
        Assert.Equal([ProductCategory.Frame, ProductCategory.Motor], model.Items.Select(i => i.Category));
        Assert.False(model.Items.Single(i => i.ProductId == _motor.Id).IsAvailable);
    }

    [Theory]
    [InlineData(' ', 3)]
    [InlineData('a', 101)]
    public void Validator_WhenNameIsBlankOrTooLong_ShouldFail(char letter, int length)
    {
        // Arrange
        string name = new(letter, length);
        var validator = new CreateBuildCommandValidator();

        // Act
        ValidationResult result = validator.Validate(
            new CreateBuildCommand(new SaveBuildModel(name, [new BuildItemModel(Guid.NewGuid(), 1)])));

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_WhenItemsRepeat_ShouldFail()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        var validator = new UpdateBuildCommandValidator();

        // Act
        ValidationResult result = validator.Validate(
            new UpdateBuildCommand(Guid.NewGuid(), new SaveBuildModel("Build", [new(id, 1), new(id, 2)])));

        // Assert
        Assert.False(result.IsValid);
    }
}
