using System.Windows.Input;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.OrderQueries;

public class GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : IQueryHandler<GetOrdersQuery, ICollection<OrderModel>>
{
    public async Task<ICollection<OrderModel>> ExecuteAsync(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOrdersByUserIdAsync(query.UserId, cancellationToken);

        return mapper.Map<ICollection<OrderModel>>(orders);
    }
}

public record GetOrdersQuery(Guid UserId);