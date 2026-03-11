using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Queries.GetOrdersByTable;

namespace NexusPOS.Application.Queries.GetPendingOrdersByDestination;

public class GetPendingOrdersByDestinationQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPendingOrdersByDestinationQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetPendingOrdersByDestinationQuery query, CancellationToken ct)
    {
        var orders = await uow.Orders.GetPendingByDestinationAsync(query.Destination, ct);
        return orders.Select(o => o.ToDto());
    }
}
