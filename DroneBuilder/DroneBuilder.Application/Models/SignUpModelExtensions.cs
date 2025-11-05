using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models;

public static class SignUpModelExtensions
{
    public static User ToEntity(this SignUpModel model)
    {
        return new User
        {
            UserName = model.Email,
            Email = model.Email
        };
    }
}