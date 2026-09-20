namespace DroneBuilder.API.Authorization;

/// <summary>
/// The access token is delivered as an HttpOnly cookie so script running on the page cannot read it.
/// Deleting a cookie only works when the options match the ones it was written with, so both sides
/// go through here.
/// </summary>
public static class AuthCookie
{
    public const string Name = "access_token";

    public static CookieOptions Options() => new()
    {
        HttpOnly = true,
        Secure = true,

        // The SPA is served from the same origin as the API, so the cookie never needs to travel
        // cross-site. Strict is what makes CSRF impossible here without separate anti-forgery tokens.
        SameSite = SameSiteMode.Strict,
        Path = "/",

        // Deliberately a session cookie: the JWT already carries its own expiry and the server
        // enforces it, so there is no second lifetime to keep in sync.
        Expires = null
    };
}
