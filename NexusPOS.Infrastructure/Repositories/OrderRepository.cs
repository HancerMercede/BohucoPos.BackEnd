using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Infrastructure.Data;

namespace NexusPOS.Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : RepositoryBase<Order>(context), IOrderRepository
{
    public async Task<Order?> GetWithItemsAsync(int orderId, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

    public async Task<IEnumerable<Order>> GetByTableAsync(string tableId, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items)
            .Where(o => o.TableId == tableId && o.Status != OrderStatus.Delivered)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetPendingByDestinationAsync(ItemDestination dest, CancellationToken ct = default)
        => await context.Orders
            .Include(o => o.Items.Where(i => i.Destination == dest))
            .Where(o => o.Items.Any(i => i.Destination == dest && i.Status != ItemStatus.Delivered && i.Status != ItemStatus.Cancelled))
            .ToListAsync(ct);
}
