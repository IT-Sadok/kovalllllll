using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.CreateCompatibilityRule;

public sealed class CreateCompatibilityRuleCommandHandler(
    ICompatibilityRepository repository,
    ICatalogMetadataRepository metadataRepository)
    : ICommandHandler<CreateCompatibilityRuleCommand, CompatibilityRuleModel>
{
    public async Task<Result<CompatibilityRuleModel>> ExecuteCommandAsync(
        CreateCompatibilityRuleCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(command.Model.Operator, true, out CompatibilityOperator compatibilityOperator))
        {
            return Result.Fail<CompatibilityRuleModel>(new ValidationError(
                $"Unsupported compatibility operator '{command.Model.Operator}'."));
        }

        string code = EntityCode.FromName(command.Model.Code);
        if (await repository.IsCodeInUseAsync(code, cancellationToken: cancellationToken))
        {
            return Result.Fail<CompatibilityRuleModel>(new ConflictError(
                $"Compatibility rule code '{code}' already exists."));
        }

        ComponentType? leftType = await metadataRepository.GetComponentTypeAsync(
            command.Model.LeftComponentTypeId, cancellationToken);
        ComponentType? rightType = await metadataRepository.GetComponentTypeAsync(
            command.Model.RightComponentTypeId, cancellationToken);
        Property? leftProperty = await metadataRepository.GetPropertyAsync(
            command.Model.LeftPropertyId, cancellationToken);
        Property? rightProperty = await metadataRepository.GetPropertyAsync(
            command.Model.RightPropertyId, cancellationToken);
        if (leftType is null || rightType is null || leftProperty is null || rightProperty is null)
        {
            return Result.Fail<CompatibilityRuleModel>(new NotFoundError(
                "One or more compatibility rule component types/properties were not found."));
        }

        var rule = new CompatibilityRule
        {
            Code = code,
            Name = command.Model.Name.Trim(),
            LeftComponentTypeId = leftType.Id,
            LeftComponentType = leftType,
            LeftPropertyId = leftProperty.Id,
            LeftProperty = leftProperty,
            RightComponentTypeId = rightType.Id,
            RightComponentType = rightType,
            RightPropertyId = rightProperty.Id,
            RightProperty = rightProperty,
            Operator = compatibilityOperator,
            FailureMessage = string.IsNullOrWhiteSpace(command.Model.FailureMessage)
                ? null
                : command.Model.FailureMessage.Trim()
        };

        try
        {
            rule.ValidateDefinition();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<CompatibilityRuleModel>(new ValidationError(exception.Message));
        }

        await repository.AddRuleAsync(rule, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(rule.ToModel());
    }
}

public sealed record CreateCompatibilityRuleCommand(CreateCompatibilityRuleModel Model);
