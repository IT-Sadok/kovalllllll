using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.OrderQueries;

public class GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper, IUserContext userContext)
    : IQueryHandler<GetOrdersQuery, PagedResult<OrderModel>>
{
    public async Task<PagedResult<OrderModel>> ExecuteAsync(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders =
            await orderRepository.GetOrdersByUserIdAsync(userContext.UserId, query.Pagination, cancellationToken);

        return new PagedResult<OrderModel>
        {
            Items = mapper.Map<IEnumerable<OrderModel>>(orders.Items),
            TotalCount = orders.TotalCount,
            Page = orders.Page,
            PageSize = orders.PageSize
        };
    }
}

public record GetOrdersQuery(PaginationParams Pagination);