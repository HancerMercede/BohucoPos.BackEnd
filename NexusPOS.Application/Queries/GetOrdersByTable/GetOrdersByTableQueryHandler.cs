using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Queries.GetOrdersByTable;

public class GetOrdersByTableQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetOrdersByTableQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByTableQuery query, CancellationToken ct)
    {
        var orders = await uow.Orders.GetByTableAsync(query.TableId, ct);
        return orders.Select(o => o.ToDto());
    }
}

public static class OrderExtensions
{
    public static OrderDto ToDto(this Domain.Entities.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderType = order.OrderType,
            Status = order.Status,
            TableId = order.TableId,
            WaiterName = order.WaiterName,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
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
        };
    }
}
