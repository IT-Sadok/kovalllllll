using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Domain.Entities;

public class User : IdentityUser
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; }
}