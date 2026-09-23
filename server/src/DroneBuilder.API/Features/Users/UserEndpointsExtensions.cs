using System.Security.Claims;
using DroneBuilder.API.Common.Authorization;
using DroneBuilder.API.Common.Extensions;
using DroneBuilder.API.Common.Routes;
using DroneBuilder.Application.Common.Mediator.Interfaces;
using DroneBuilder.Application.Features.Users;
using DroneBuilder.Application.Features.Users.ConfirmEmail;
using DroneBuilder.Application.Features.Users.ResendEmailConfirmation;
using DroneBuilder.Application.Features.Users.SignIn;
using DroneBuilder.Application.Features.Users.SignUp;
using DroneBuilder.Infrastructure.Options;
using FluentResults;
using Microsoft.Extensions.Options;
namespace DroneBuilder.API.Features.Users;

public static class UserEndpointsExtensions
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Users.SignUp,
            async (IMediator mediator, SignUpModel model, CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(new SignUpUserCommand(model), cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Users")
            .RequireRateLimiting(RateLimitingExtension.EmailPolicy);

        app.MapPost(ApiRoutes.Users.ResendConfirmation,
            async (IMediator mediator, ResendEmailConfirmationModel model, CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(
                    new ResendEmailConfirmationCommand(model.Email),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Users")
            .RequireRateLimiting(RateLimitingExtension.EmailPolicy);

        app.MapPost(ApiRoutes.Users.SignIn,
            async (IMediator mediator, HttpContext httpContext, IOptions<JwtOptions> jwtOptions, SignInModel model,
                CancellationToken cancellationToken) =>
            {
                Result<AuthUserModel> result = await mediator.ExecuteCommandAsync<SignInCommand, AuthUserModel>(
                    new SignInCommand(model.Email, model.Password),
                    cancellationToken);

                if (result.IsFailed)
                {
                    return result.ToHttpResult();
                }

                httpContext.Response.Cookies.Append(
                    AuthCookie.Name,
                    result.Value.AccessToken,
                    AuthCookie.Options(DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.ExpiryMinutes)));

                return Results.NoContent();
            }).WithTags("Users")
            .RequireRateLimiting(RateLimitingExtension.LoginPolicy);

        app.MapPost(ApiRoutes.Users.SignOut,
            (HttpContext httpContext) =>
            {
                httpContext.Response.Cookies.Delete(AuthCookie.Name, AuthCookie.Options());
                return Results.NoContent();
            }).WithTags("Users");

        app.MapGet(ApiRoutes.Users.Me,
            (ClaimsPrincipal principal) =>
            {
                var currentUser = new CurrentUserModel
                {
                    Id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    Roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
                };

                return Results.Ok(currentUser);
            }).WithTags("Users")
            .RequireAuthorization();

        app.MapGet(ApiRoutes.Users.ConfirmEmail,
            async (IMediator mediator, Guid userId, string token, CancellationToken cancellationToken) =>
            {
                Result result = await mediator.ExecuteCommandAsync(
                    new ConfirmEmailCommand(userId, token),
                    cancellationToken);
                return result.ToHttpResult();
            }).WithTags("Users");

        return app;
    }
}
