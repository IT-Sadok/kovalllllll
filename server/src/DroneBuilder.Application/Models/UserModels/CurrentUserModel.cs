namespace DroneBuilder.Application.Models.UserModels;

public class CurrentUserModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}
