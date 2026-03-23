using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string IdDisplay => $"ORD-{Id:D3}";
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? TableId { get; private set; }
    public int? TabId { get; set; }
    public Tab? Tab { get; set; }
    public string WaiterName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(OrderType type, string? tableId, string waiterName, int? tabId = null)
    {
        return new Order
        {
            OrderType = type,
            Status = OrderStatus.Pending,
            TableId = tableId,
            TabId = tabId,
            WaiterName = waiterName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(string productId, string productName, decimal unitPrice, int qty, string? notes, ItemDestination dest)
    {
        _items.Add(new OrderItem
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = qty,
            Notes = notes,
            Destination = dest,
            Status = ItemStatus.Pending
        });
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }
}
