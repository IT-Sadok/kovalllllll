using DroneBuilder.Application.Models.UserModels;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class UserMaping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<string, AuthUserModel>()
            .MapWith(src => new AuthUserModel { AccessToken = src });
    }
}