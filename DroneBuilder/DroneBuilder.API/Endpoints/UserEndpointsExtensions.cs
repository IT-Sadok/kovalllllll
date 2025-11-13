using DroneBuilder.API.Endpoints.Routes;
using DroneBuilder.Application.Mediator.Commands;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;

namespace DroneBuilder.API.Endpoints;

public static class UserEndpointsExtensions
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Users.SignUp,
            async (IMediator mediator, SignUpModel model, CancellationToken cancellationToken) =>
                await mediator.ExecuteCommandAsync(new SignUpUserCommand(model), cancellationToken));

        app.MapPost(ApiRoutes.Users.SignIn,
            async (IMediator mediator, SignInModel model, CancellationToken cancellationToken) =>
            {
                var result = await mediator.ExecuteCommandAsync<SignInCommand, AuthUserModel>(
                    new SignInCommand(model.Email, model.Password),
                    cancellationToken);
                return Results.Ok(result);
            });
    }
}