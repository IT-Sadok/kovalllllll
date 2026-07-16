using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.API.Extensions;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;
using FluentResults;

namespace DroneBuilder.API.Endpoints;

public static class UserEndpointsExtensions
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Users.SignUp,
            async (IMediator mediator, SignUpModel model, CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new SignUpUserCommand(model), cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Users");

        app.MapPost(ApiRoutes.Users.SignIn,
            async (IMediator mediator, SignInModel model, CancellationToken cancellationToken) =>
            {
                Result<AuthUserModel> result = await mediator.ExecuteCommandAsync<SignInCommand, AuthUserModel>(
                    new SignInCommand(model.Email, model.Password),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Users")
            .RequireRateLimiting("LoginPolicy");

        return app;
    }
}
