using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Queries.GetTabDetails;

public class GetTabDetailsQueryHandler(IUnitOfWork uow) 
    : IRequestHandler<GetTabDetailsQuery, TabDto?>
{
    public async Task<TabDto?> Handle(GetTabDetailsQuery request, CancellationToken ct)
    {
        var tab = await uow.Tabs.GetByIdAsync(request.TabId, ct);
        if (tab == null) return null;

        var orders = await uow.Tabs.GetOrdersByTabIdAsync(request.TabId, ct);
            
        decimal subtotal = 0;
        foreach (var order in orders)
        {
            foreach (var item in order.Items.Where(i => i.Status != ItemStatus.Cancelled))
            {
                subtotal += item.UnitPrice * item.Quantity;
            }
        }

        return new TabDto
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
        };
    }
}
