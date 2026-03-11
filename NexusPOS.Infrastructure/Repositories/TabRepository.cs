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
        var tab = await _context.Tabs.FindAsync(new object[] { tabId }, ct);
        if (tab == null) return null;

        var orders = await _context.Orders
            .Where(o => o.TabId == tabId)
            .Include(o => o.Items)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);

        var tabWithOrders = Tab.Open(tab.Location, tab.CustomerName, tab.WaiterId, tab.WaiterName, tab.TaxRate);
        typeof(Tab).GetProperty("Id")!.SetValue(tabWithOrders, tab.Id);
        typeof(Tab).GetProperty("Status")!.SetValue(tabWithOrders, tab.Status);
        typeof(Tab).GetProperty("OpenedAt")!.SetValue(tabWithOrders, tab.OpenedAt);
        typeof(Tab).GetProperty("ClosedAt")!.SetValue(tabWithOrders, tab.ClosedAt);
        typeof(Tab).GetProperty("PaymentMethod")!.SetValue(tabWithOrders, tab.PaymentMethod);
        typeof(Tab).GetProperty("Notes")!.SetValue(tabWithOrders, tab.Notes);

        foreach (var order in orders)
        {
            tabWithOrders.AddOrder(order);
        }

        return tabWithOrders;
    }

    public async Task<IEnumerable<Tab>> GetActiveByLocationAsync(string? location, CancellationToken ct = default)
    {
        var query = _context.Tabs
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
        return await _context.Tabs
            .Where(t => t.Status == TabStatus.Open || t.Status == TabStatus.Pending)
            .OrderBy(t => t.OpenedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<string>> GetLocationsWithOpenTabsAsync(CancellationToken ct = default)
    {
        return await _context.Tabs
            .Where(t => t.Status == TabStatus.Open || t.Status == TabStatus.Pending)
            .Select(t => t.Location)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetOrdersByTabIdAsync(int tabId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.TabId == tabId)
            .Include(o => o.Items)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }
}
