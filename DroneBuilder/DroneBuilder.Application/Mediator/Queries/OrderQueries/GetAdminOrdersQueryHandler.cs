using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using MapsterMapper;

namespace DroneBuilder.Application.Mediator.Queries.OrderQueries;

public class GetAdminOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    : IQueryHandler<GetAdminOrdersQuery, PagedResult<OrderModel>>
{
    public async Task<PagedResult<OrderModel>> ExecuteAsync(GetAdminOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetPagedOrdersAsync(query.Pagination, cancellationToken);

        return new PagedResult<OrderModel>
        {
            Items = mapper.Map<IEnumerable<OrderModel>>(orders.Items),
            TotalCount = orders.TotalCount,
            Page = orders.Page,
            PageSize = orders.PageSize
        };
    }
}

public record GetAdminOrdersQuery(PaginationParams Pagination);
