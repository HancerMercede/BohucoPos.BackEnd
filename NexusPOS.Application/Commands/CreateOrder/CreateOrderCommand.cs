using MediatR;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<int>
{
    public OrderType OrderType { get; init; }
    public string? TableId { get; init; }
    public string WaiterName { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = new();
}

public record OrderItemDto
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
