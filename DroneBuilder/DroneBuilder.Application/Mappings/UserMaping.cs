using DroneBuilder.Application.Models;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class UserMaping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<string, AuthUserModel>()
            .Map(dest => dest.AccessToken, src => src);
    }
}