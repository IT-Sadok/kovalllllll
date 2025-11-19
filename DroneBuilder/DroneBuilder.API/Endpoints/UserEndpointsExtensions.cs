using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands.UserCommands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.UserModels;

namespace DroneBuilder.API.Endpoints;

public static class UserEndpointsExtensions
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Users.SignUp,
            async (IMediator mediator, SignUpModel model, CancellationToken cancellationToken) =>
                await mediator.ExecuteCommandAsync(new SignUpUserCommand(model), cancellationToken)).WithTags("Users");

        app.MapPost(ApiRoutes.Users.SignIn,
            async (IMediator mediator, SignInModel model, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<SignInCommand, AuthUserModel>(
                    new SignInCommand(model.Email, model.Password),
                    cancellationToken);
                return Results.Ok(result);
            }).WithTags("Users");

        return app;
    }
}