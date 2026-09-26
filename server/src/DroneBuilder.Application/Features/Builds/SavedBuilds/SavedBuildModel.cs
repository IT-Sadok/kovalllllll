using DroneBuilder.Application.Features.Builds.CheckBuild;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Features.Builds.SavedBuilds;

public record SaveBuildModel(string Name, List<BuildItemModel> Items);

public record SavedBuildModel(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    decimal TotalPrice,
    IReadOnlyList<SavedBuildItemModel> Items);

public record SavedBuildItemModel(
    Guid ProductId,
    string ProductName,
    ProductCategory Category,
    decimal Price,
    int Quantity,
    string? ImageUrl,
    bool IsAvailable);
