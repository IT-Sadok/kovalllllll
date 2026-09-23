using DroneBuilder.Application.Mappings;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
namespace DroneBuilder.Application.Mediator.Queries.OrderQueries;

public class GetAdminOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetAdminOrdersQuery, PagedResult<OrderModel>>
{
    public async Task<Result<PagedResult<OrderModel>>> ExecuteAsync(GetAdminOrdersQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Order> orders = await orderRepository.GetPagedOrdersAsync(query.Pagination, cancellationToken);

        return Result.Ok(new PagedResult<OrderModel>
        {
            Items = orders.Items.Select(o => o.ToModel()).ToList(),
            TotalCount = orders.TotalCount,
            Page = orders.Page,
            PageSize = orders.PageSize
        });
    }
}

public record GetAdminOrdersQuery(PaginationParams Pagination);
