namespace NexusPOS.Application.DTOs;

public class SalesAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageTicket { get; set; }
    public List<DailySalesDto> DailySales { get; set; } = new();
    public List<ProductSalesDto> TopProducts { get; set; } = new();
    public List<WaiterPerformanceDto> WaiterPerformance { get; set; } = new();
    public List<PaymentMethodDto> PaymentBreakdown { get; set; } = new();
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageTicket { get; set; }
}

public class ProductSalesDto
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class WaiterPerformanceDto
{
    public string WaiterName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class PaymentMethodDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Total { get; set; }
}
