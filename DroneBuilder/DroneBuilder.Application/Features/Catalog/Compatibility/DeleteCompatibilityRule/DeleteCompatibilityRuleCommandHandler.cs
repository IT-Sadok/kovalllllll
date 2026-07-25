using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;

namespace DroneBuilder.Application.Features.Catalog.Compatibility.DeleteCompatibilityRule;

public sealed class DeleteCompatibilityRuleCommandHandler(ICompatibilityRepository repository)
    : ICommandHandler<DeleteCompatibilityRuleCommand>
{
    public async Task<Result> ExecuteCommandAsync(
        DeleteCompatibilityRuleCommand command,
        CancellationToken cancellationToken)
    {
        CompatibilityRule? rule = await repository.GetRuleAsync(command.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Fail(new NotFoundError(
                $"Compatibility rule with id {command.RuleId} not found."));
        }

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed record DeleteCompatibilityRuleCommand(Guid RuleId);
