namespace DroneBuilder.API.Authorization;

public static class AuthCookie
{
    public const string Name = "access_token";

    public static CookieOptions Options() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = null
    };
}
