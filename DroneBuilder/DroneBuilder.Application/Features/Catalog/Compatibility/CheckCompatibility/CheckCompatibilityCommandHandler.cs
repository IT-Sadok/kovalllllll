using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.CheckCompatibility;

public sealed class CheckCompatibilityCommandHandler(ICompatibilityRepository repository)
    : ICommandHandler<CheckCompatibilityCommand, CompatibilityCheckResultModel>
{
    public async Task<Result<CompatibilityCheckResultModel>> ExecuteCommandAsync(
        CheckCompatibilityCommand command,
        CancellationToken cancellationToken)
    {
        ProductVariant? left = await repository.GetVariantAsync(
            command.Model.LeftVariantId, cancellationToken);
        ProductVariant? right = await repository.GetVariantAsync(
            command.Model.RightVariantId, cancellationToken);
        if (left is null || right is null)
        {
            return Result.Fail<CompatibilityCheckResultModel>(new NotFoundError(
                "One or both product variants were not found or are not published."));
        }

        Guid? leftTypeId = left.Product?.ComponentTypeId;
        Guid? rightTypeId = right.Product?.ComponentTypeId;
        if (!leftTypeId.HasValue || !rightTypeId.HasValue)
        {
            return Result.Ok(new CompatibilityCheckResultModel
            {
                Status = "Unknown",
                IsCompatible = null,
                LeftVariantId = left.Id,
                RightVariantId = right.Id,
                Rules = []
            });
        }

        ICollection<CompatibilityRule> rules = await repository.GetApplicableRulesAsync(
            leftTypeId.Value,
            rightTypeId.Value,
            cancellationToken);
        return Result.Ok(CompatibilityEvaluator.Evaluate(left, right, rules));
    }
}

public sealed record CheckCompatibilityCommand(CheckCompatibilityModel Model);
