using DroneBuilder.API.Authorization;
using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Features.Catalog.Compatibility.CheckCompatibility;
using DroneBuilder.Application.Features.Catalog.Compatibility.CreateCompatibilityRule;
using DroneBuilder.Application.Features.Catalog.Compatibility.DeleteCompatibilityRule;
using DroneBuilder.Application.Features.Catalog.Compatibility.GetCompatibilityRules;
using DroneBuilder.Application.Features.Catalog.Compatibility.Models;
using DroneBuilder.Application.Features.Catalog.Compatibility.UpdateCompatibilityRule;
using DroneBuilder.Application.Mediator.Interfaces;
using FluentResults;

namespace DroneBuilder.API.Features.Catalog.Compatibility;

public static class CompatibilityEndpointExtensions
{
    public static IEndpointRouteBuilder MapCompatibilityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Compatibility.Check,
                async (CheckCompatibilityModel model, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<CompatibilityCheckResultModel> result =
                        await mediator.ExecuteCommandAsync<
                            CheckCompatibilityCommand,
                            CompatibilityCheckResultModel>(
                            new CheckCompatibilityCommand(model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Compatibility");

        app.MapGet(ApiRoutes.Compatibility.Rules,
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<ICollection<CompatibilityRuleModel>> result =
                        await mediator.ExecuteQueryAsync<
                            GetCompatibilityRulesQuery,
                            ICollection<CompatibilityRuleModel>>(
                            new GetCompatibilityRulesQuery(), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Compatibility")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPost(ApiRoutes.Compatibility.Rules,
                async (CreateCompatibilityRuleModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<CompatibilityRuleModel> result =
                        await mediator.ExecuteCommandAsync<
                            CreateCompatibilityRuleCommand,
                            CompatibilityRuleModel>(
                            new CreateCompatibilityRuleCommand(model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Compatibility")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapPatch(ApiRoutes.Compatibility.RuleById,
                async (Guid ruleId, UpdateCompatibilityRuleModel model,
                    IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result<CompatibilityRuleModel> result =
                        await mediator.ExecuteCommandAsync<
                            UpdateCompatibilityRuleCommand,
                            CompatibilityRuleModel>(
                            new UpdateCompatibilityRuleCommand(ruleId, model), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Compatibility")
            .RequireAuthorization(PolicyNames.Admin);

        app.MapDelete(ApiRoutes.Compatibility.RuleById,
                async (Guid ruleId, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    Result result = await mediator.ExecuteCommandAsync(
                        new DeleteCompatibilityRuleCommand(ruleId), cancellationToken);
                    return result.ToHttpResult();
                })
            .WithTags("Compatibility")
            .RequireAuthorization(PolicyNames.Admin);

        return app;
    }
}
