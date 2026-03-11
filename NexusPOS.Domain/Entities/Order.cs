using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public class Order
{
    private static int _orderSequence = 0;
    private static int _itemSequence = 0;
    private static readonly object _lock = new();

    public int Id { get; private set; }
    public string IdDisplay => $"ORD-{Id:D3}";
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? TableId { get; private set; }
    public int? TabId { get; set; }
    public string WaiterName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static void SetSequence(int value) => _orderSequence = value;
    public static void SetItemSequence(int value) => _itemSequence = value;

    public static int GenerateNextId()
    {
        lock (_lock)
        {
            _orderSequence++;
            return _orderSequence;
        }
    }

    public static int GenerateNextItemId()
    {
        lock (_lock)
        {
            _itemSequence++;
            return _itemSequence;
        }
    }

    public static Order Create(OrderType type, string? tableId, string waiterName, int? tabId = null)
    {
        var id = GenerateNextId();
        return new Order
        {
            Id = id,
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
            Id = GenerateNextItemId(),
            OrderId = Id,
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
