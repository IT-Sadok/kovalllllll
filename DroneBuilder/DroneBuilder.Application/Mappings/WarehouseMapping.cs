using DroneBuilder.Application.Models.WarehouseModels;
using DroneBuilder.Domain.Entities;
using Mapster;

namespace DroneBuilder.Application.Mappings;

public class WarehouseMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateWarehouseItemModel, WarehouseItem>()
            .Map(dest => dest.WarehouseId, src => src.WarehouseId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Warehouse)
            .Ignore(dest => dest.Product);

        config.NewConfig<WarehouseItem, WarehouseItemModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.WarehouseId, src => src.WarehouseId)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.Quantity, src => src.Quantity);

        config.NewConfig<Warehouse, ICollection<WarehouseItemModel>>()
            .Map(dest => dest, src => src);

        config.NewConfig<Warehouse, WarehouseModel>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}