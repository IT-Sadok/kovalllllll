using DroneBuilder.Application.Common;
using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.UpdateCompatibilityRule;

public sealed class UpdateCompatibilityRuleCommandHandler(ICompatibilityRepository repository)
    : ICommandHandler<UpdateCompatibilityRuleCommand, CompatibilityRuleModel>
{
    public async Task<Result<CompatibilityRuleModel>> ExecuteCommandAsync(
        UpdateCompatibilityRuleCommand command,
        CancellationToken cancellationToken)
    {
        CompatibilityRule? rule = await repository.GetRuleAsync(command.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Fail<CompatibilityRuleModel>(new NotFoundError(
                $"Compatibility rule with id {command.RuleId} not found."));
        }

        CompatibilityOperator targetOperator = rule.Operator;
        if (command.Model.Operator is not null &&
            !Enum.TryParse(command.Model.Operator, true, out targetOperator))
        {
            return Result.Fail<CompatibilityRuleModel>(new ValidationError(
                $"Unsupported compatibility operator '{command.Model.Operator}'."));
        }

        string targetCode = command.Model.Code is null ? rule.Code : EntityCode.FromName(command.Model.Code);
        if (await repository.IsCodeInUseAsync(targetCode, rule.Id, cancellationToken))
        {
            return Result.Fail<CompatibilityRuleModel>(new ConflictError(
                $"Compatibility rule code '{targetCode}' already exists."));
        }

        rule.Code = targetCode;
        rule.Name = command.Model.Name?.Trim() ?? rule.Name;
        rule.Operator = targetOperator;
        rule.FailureMessage = command.Model.ClearFailureMessage
            ? null
            : command.Model.FailureMessage?.Trim() ?? rule.FailureMessage;
        rule.IsActive = command.Model.IsActive ?? rule.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;

        try
        {
            rule.ValidateDefinition();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Fail<CompatibilityRuleModel>(new ValidationError(exception.Message));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok(rule.ToModel());
    }
}

public sealed record UpdateCompatibilityRuleCommand(Guid RuleId, UpdateCompatibilityRuleModel Model);
