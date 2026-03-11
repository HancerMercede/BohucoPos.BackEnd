using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Queries.GetPendingOrdersByDestination;

public record GetPendingOrdersByDestinationQuery : IRequest<IEnumerable<OrderDto>>
{
    public ItemDestination Destination { get; init; }
}
