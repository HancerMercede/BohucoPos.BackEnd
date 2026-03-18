using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetSalesAnalytics;

public class GetSalesAnalyticsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSalesAnalyticsQuery, SalesAnalyticsDto>
{
    public async Task<SalesAnalyticsDto> Handle(GetSalesAnalyticsQuery request, CancellationToken ct)
    {
        var startDate = (request.StartDate ?? DateTime.UtcNow.AddDays(-7).Date).ToUniversalTime();
        var endDate = (request.EndDate ?? DateTime.UtcNow).ToUniversalTime();

        var closedTabs = await unitOfWork.Tabs.GetClosedTabsWithOrdersAsync(startDate, endDate, ct);
        var tabList = closedTabs.ToList();

        var calculateTotal = (Domain.Entities.Tab tab) =>
        {
            var subtotal = tab.Orders.SelectMany(o => o.Items).Sum(i => i.UnitPrice * i.Quantity);
            return subtotal * 1.18m;
        };

        var dailySales = tabList
            .GroupBy(t => t.ClosedAt!.Value.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                TotalRevenue = g.Sum(t => calculateTotal(t)),
                OrderCount = g.Sum(t => t.Orders.Count),
                AverageTicket = g.Average(t => calculateTotal(t))
            })
            .OrderBy(d => d.Date)
            .ToList();

        var productSales = tabList
            .SelectMany(t => t.Orders)
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

        var waiterSales = tabList
            .GroupBy(t => t.WaiterName)
            .Select(g => new WaiterPerformanceDto
            {
                WaiterName = g.Key,
                OrderCount = g.Sum(t => t.Orders.Count),
                TotalRevenue = g.Sum(t => calculateTotal(t))
            })
            .OrderByDescending(w => w.TotalRevenue)
            .ToList();

        var paymentBreakdown = tabList
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new PaymentMethodDto
            {
                Method = g.Key?.ToString() ?? "Unknown",
                Count = g.Count(),
                Total = g.Sum(t => calculateTotal(t))
            })
            .ToList();

        return new SalesAnalyticsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = tabList.Sum(t => calculateTotal(t)),
            TotalOrders = tabList.Sum(t => t.Orders.Count),
            AverageTicket = tabList.Any() ? tabList.Average(t => calculateTotal(t)) : 0,
            DailySales = dailySales,
            TopProducts = productSales,
            WaiterPerformance = waiterSales,
            PaymentBreakdown = paymentBreakdown
        };
    }
}
