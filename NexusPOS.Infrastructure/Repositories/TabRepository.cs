using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Infrastructure.Data;

namespace NexusPOS.Infrastructure.Repositories;

public class TabRepository(AppDbContext context) : RepositoryBase<Tab>(context), ITabRepository
{
    public async Task<Tab?> GetWithOrdersAsync(int tabId, CancellationToken ct = default)
    {
        var tab = await context.Tabs
            .Include(t => t.Orders)
            .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(t => t.Id == tabId, ct);

        return tab;
    }

    public async Task<IEnumerable<Tab>> GetActiveByLocationAsync(string? location, CancellationToken ct = default)
    {
        var query = context.Tabs
            .Where(t => t.Status != TabStatus.Closed && t.Status != TabStatus.Cancelled);

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(t => t.Location == location);
        }

        return await query
            .OrderBy(t => t.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Tab>> GetOpenTabsAsync(CancellationToken ct = default)
    {
        return await context.Tabs
            .Where(t => t.Status == TabStatus.Open || t.Status == TabStatus.Pending)
            .OrderBy(t => t.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<string>> GetLocationsWithOpenTabsAsync(CancellationToken ct = default)
    {
        return await context.Tabs
            .Where(t => t.Status == TabStatus.Open || t.Status == TabStatus.Pending)
            .Select(t => t.Location)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetOrdersByTabIdAsync(int tabId, CancellationToken ct = default)
    {
        return await context.Orders
            .Where(o => o.TabId == tabId)
            .Include(o => o.Items)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Tab>> GetClosedTabsWithOrdersAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await context.Tabs
            .Include(t => t.Orders)
            .ThenInclude(o => o.Items)
            .Where(t => t.Status == TabStatus.Closed && t.ClosedAt >= startDate && t.ClosedAt <= endDate)
            .ToListAsync(ct);
    }
}
