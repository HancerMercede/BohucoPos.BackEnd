using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.DTOs;

public record OrderDto
{
    public int Id { get; init; }
    public string IdDisplay => $"ORD-{Id:D3}";
    public OrderType OrderType { get; init; }
    public OrderStatus Status { get; init; }
    public string? TableId { get; init; }
    public string WaiterName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
}

public record OrderItemDto
{
    public int Id { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
    public ItemDestination Destination { get; init; }
    public ItemStatus Status { get; init; }
}
