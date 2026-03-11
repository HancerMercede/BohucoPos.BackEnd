namespace NexusPOS.Application.DTOs;

public class TabBillData
{
    public string TabId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string WaiterName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime RequestedAt { get; set; }
    public List<BillItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public class BillItem
{
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Notes { get; init; }
    public decimal Total => Quantity * UnitPrice;
}
