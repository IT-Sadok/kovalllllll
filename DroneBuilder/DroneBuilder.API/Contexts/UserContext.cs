using System.Security.Claims;
using DroneBuilder.Application.Contexts;

namespace DroneBuilder.API.Contexts;

public class UserContext(IHttpContextAccessor contextAccessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            Claim? userIdClaim = contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim?.Value is null ||
                !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException(
                    "User is not authenticated or the NameIdentifier claim is invalid.");
            }

            return userId;
        }
    }

    public string UserEmail
        => contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
}
