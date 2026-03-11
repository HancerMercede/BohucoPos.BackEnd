using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetActiveTabsByLocation;

public class GetActiveTabsByLocationQueryHandler(ITabRepository tabRepository) 
    : IRequestHandler<GetActiveTabsByLocationQuery, IEnumerable<TabDto>>
{
    public async Task<IEnumerable<TabDto>> Handle(GetActiveTabsByLocationQuery request, CancellationToken ct)
    {
        var tabs = await tabRepository.GetActiveByLocationAsync(request.Location, ct);
        
        var result = new List<TabDto>();
        
        foreach (var tab in tabs)
        {
            var orders = await tabRepository.GetOrdersByTabIdAsync(tab.Id, ct);
            
            decimal subtotal = 0;
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    subtotal += item.UnitPrice * item.Quantity;
                }
            }
            
            result.Add(new TabDto
            {
                Id = tab.Id,
                IdDisplay = tab.IdDisplay,
                Location = tab.Location,
                CustomerName = tab.CustomerName,
                WaiterName = tab.WaiterName,
                Status = tab.Status,
                OpenedAt = tab.OpenedAt,
                ClosedAt = tab.ClosedAt,
                PaymentMethod = tab.PaymentMethod,
                Subtotal = subtotal,
                Tax = subtotal * tab.TaxRate,
                Total = subtotal * (1 + tab.TaxRate),
                Notes = tab.Notes,
                Orders = orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderType = o.OrderType,
                    Status = o.Status,
                    TableId = o.TableId,
                    WaiterName = o.WaiterName,
                    CreatedAt = o.CreatedAt,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        Notes = i.Notes,
                        Destination = i.Destination,
                        Status = i.Status
                    }).ToList()
                }).ToList()
            });
        }
        
        return result;
    }
}
