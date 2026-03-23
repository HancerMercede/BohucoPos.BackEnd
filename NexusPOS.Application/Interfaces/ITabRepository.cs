using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Interfaces;

public interface ITabRepository : IRepositoryBase<Tab>
{
    Task<Tab?> GetWithOrdersAsync(int tabId, CancellationToken ct = default);
    Task<IEnumerable<Tab>> GetActiveByLocationAsync(string location, CancellationToken ct = default);
    Task<IEnumerable<Tab>> GetOpenTabsAsync(CancellationToken ct = default);
    Task<IEnumerable<string>> GetLocationsWithOpenTabsAsync(CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOrdersByTabIdAsync(int tabId, CancellationToken ct = default);
    Task<IEnumerable<Tab>> GetClosedTabsWithOrdersAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
}
