using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public class Tab
{
    public int Id { get; private set; }
    public string IdDisplay => $"TAB-{Id:D3}";
    public string Location { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string WaiterId { get; private set; } = string.Empty;
    public string WaiterName { get; private set; } = string.Empty;
    public TabStatus Status { get; set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal TaxRate { get; private set; } = 0.18m;
    public string? Notes { get; set; }

    public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

    public decimal Subtotal => Orders.SelectMany(o => o.Items).Sum(i => i.UnitPrice * i.Quantity);
    public decimal Tax => Subtotal * TaxRate;
    public decimal Total => Subtotal + Tax;

    private Tab() { }

    public static Tab Open(string location, string customerName, string waiterId, string waiterName, decimal taxRate = 0.18m)
    {
        return new Tab
        {
            Location = location,
            CustomerName = customerName,
            WaiterId = waiterId,
            WaiterName = waiterName,
            Status = TabStatus.Open,
            OpenedAt = DateTime.UtcNow,
            TaxRate = taxRate
        };
    }

    public void AddOrder(Order order)
    {
        if (Status != TabStatus.Open)
            throw new InvalidOperationException("Cannot add order to a tab that is not open");
        Orders.Add(order);
    }

    public void RequestBill()
    {
        if (Status != TabStatus.Open)
            throw new InvalidOperationException("Cannot request bill for a tab that is not open");
        Status = TabStatus.Pending;
    }

    public void ContinueOrdering()
    {
        if (Status != TabStatus.Pending)
            throw new InvalidOperationException("Tab must be pending to continue ordering");
        Status = TabStatus.Open;
    }

    public void Close(PaymentMethod paymentMethod)
    {
        if (Status != TabStatus.Pending && Status != TabStatus.Open)
            throw new InvalidOperationException("Tab must be open or pending to close");
        Status = TabStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        PaymentMethod = paymentMethod;
    }

    public void CloseDirect(PaymentMethod paymentMethod)
    {
        Status = TabStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        PaymentMethod = paymentMethod;
    }

    public void Cancel(string? reason = null)
    {
        Status = TabStatus.Cancelled;
        ClosedAt = DateTime.UtcNow;
        Notes = reason;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
    }
}
