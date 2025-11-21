using DroneBuilder.Domain.Entities;

namespace DroneBuilder.Application.Repositories;

public interface IOrderRepository
{
    Task CreateOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task<ICollection<Order>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}