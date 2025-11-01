using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models;

public class SignUpModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public User ToEntity()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        return new User
        {
            UserName = UserName,
            Email = Email,
            Account = account
        };
    }
}