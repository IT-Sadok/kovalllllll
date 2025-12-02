using DroneBuilder.Application.Models;
using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IOrderRepository
{
    Task CreateOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<PagedResult<Order>> GetOrdersByUserIdAsync(
        Guid userId,
        PaginationParams pagination,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}