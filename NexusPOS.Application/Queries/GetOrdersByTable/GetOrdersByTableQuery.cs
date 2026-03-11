using MediatR;
using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Queries.GetOrdersByTable;

public record GetOrdersByTableQuery : IRequest<IEnumerable<OrderDto>>
{
    public string TableId { get; init; } = string.Empty;
}
