using DroneBuilder.Application.Contexts;
using DroneBuilder.Application.Mediator.Interfaces;
using DroneBuilder.Application.Models;
using DroneBuilder.Application.Models.OrderModels;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Domain.Entities;
using FluentResults;
using DroneBuilder.Application.Mappings;
namespace DroneBuilder.Application.Mediator.Queries.OrderQueries;

public class GetOrdersQueryHandler(IOrderRepository orderRepository, IUserContext userContext)
    : IQueryHandler<GetOrdersQuery, PagedResult<OrderModel>>
{
    public async Task<Result<PagedResult<OrderModel>>> ExecuteAsync(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        PagedResult<Order> orders =
            await orderRepository.GetOrdersByUserIdAsync(userContext.UserId, query.Pagination, cancellationToken);

        return Result.Ok(new PagedResult<OrderModel>
        {
            Items = orders.Items.Select(o => o.ToModel()).ToList(),
            TotalCount = orders.TotalCount,
            Page = orders.Page,
            PageSize = orders.PageSize
        });
    }
}

public record GetOrdersQuery(PaginationParams Pagination);
