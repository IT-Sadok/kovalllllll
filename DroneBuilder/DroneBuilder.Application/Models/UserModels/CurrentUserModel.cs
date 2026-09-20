namespace DroneBuilder.Application.Models.UserModels;

/// <summary>
/// What the signed-in browser is allowed to know about itself. With the token in an HttpOnly cookie
/// the client can no longer decode it, so the identity has to be handed over explicitly.
/// </summary>
public class CurrentUserModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}
