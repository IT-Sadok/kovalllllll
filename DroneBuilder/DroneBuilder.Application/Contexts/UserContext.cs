using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Contexts;

public class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId
        => Guid.Parse(contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      Guid.Empty.ToString());

    public string UserEmail
        => contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
}