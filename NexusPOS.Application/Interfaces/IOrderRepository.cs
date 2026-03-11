using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Interfaces;

public interface IOrderRepository : IRepositoryBase<Order>
{
    Task<Order?> GetWithItemsAsync(int orderId, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByTableAsync(string tableId, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetPendingByDestinationAsync(ItemDestination dest, CancellationToken ct = default);
}
