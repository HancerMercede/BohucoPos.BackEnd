using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public class OrderItem
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
    public ItemDestination Destination { get; init; }
    public ItemStatus Status { get; set; }
}
