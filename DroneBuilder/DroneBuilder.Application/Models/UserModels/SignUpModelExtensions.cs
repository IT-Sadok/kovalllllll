using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Models.UserModels;

public static class SignUpModelExtensions
{
    public static User ToEntity(this SignUpModel model) => new()
    {
        UserName = model.Email,
        Email = model.Email
    };
}