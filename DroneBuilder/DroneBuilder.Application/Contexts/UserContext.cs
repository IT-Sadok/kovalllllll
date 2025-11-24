using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DroneBuilder.Application.Contexts;

public class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var userIdClaim = contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim?.Value == null)
                throw new UnauthorizedAccessException("User is not authenticated or NameIdentifier claim missing");

            return Guid.Parse(userIdClaim.Value);
        }
    }


    public string UserEmail
        => contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
}