using Microsoft.AspNetCore.Identity;

namespace DroneBuilder.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public Cart? Cart { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
}