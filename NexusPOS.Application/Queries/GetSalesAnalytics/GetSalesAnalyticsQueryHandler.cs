using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetSalesAnalytics;

public class GetSalesAnalyticsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSalesAnalyticsQuery, SalesAnalyticsDto>
{
    public async Task<SalesAnalyticsDto> Handle(GetSalesAnalyticsQuery request, CancellationToken ct)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.Date.AddDays(-7);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var tabs = await unitOfWork.Tabs.GetAllAsync(ct);
        var orders = await unitOfWork.Orders.GetAllAsync(ct);

        var closedTabs = tabs
            .Where(t => t.Status == Domain.Enums.TabStatus.Closed && 
                       t.ClosedAt >= startDate && 
                       t.ClosedAt <= endDate)
            .ToList();

        var tabOrders = orders
            .Where(o => o.TabId.HasValue && closedTabs.Any(t => t.Id == o.TabId))
            .ToList();

        var dailySales = closedTabs
            .GroupBy(t => t.ClosedAt!.Value.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                TotalRevenue = g.Sum(t => t.Total),
                OrderCount = g.Count(),
                AverageTicket = g.Average(t => t.Total)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var productSales = tabOrders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductName)
            .Select(g => new ProductSalesDto
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        var waiterSales = closedTabs
            .GroupBy(t => t.WaiterName)
            .Select(g => new WaiterPerformanceDto
            {
                WaiterName = g.Key,
                OrderCount = tabOrders.Count(o => o.WaiterName == g.Key),
                TotalRevenue = g.Sum(t => t.Total)
            })
            .OrderByDescending(w => w.TotalRevenue)
            .ToList();

        var paymentBreakdown = closedTabs
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new PaymentMethodDto
            {
                Method = g.Key?.ToString() ?? "Unknown",
                Count = g.Count(),
                Total = g.Sum(t => t.Total)
            })
            .ToList();

        return new SalesAnalyticsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = closedTabs.Sum(t => t.Total),
            TotalOrders = closedTabs.Count,
            AverageTicket = closedTabs.Any() ? closedTabs.Average(t => t.Total) : 0,
            DailySales = dailySales,
            TopProducts = productSales,
            WaiterPerformance = waiterSales,
            PaymentBreakdown = paymentBreakdown
        };
    }
}
