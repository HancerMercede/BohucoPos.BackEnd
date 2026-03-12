using NexusPOS.Domain.Enums;

namespace NexusPOS.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public ItemDestination Destination { get; set; }
    public ProductType ProductType { get; set; }
    public int? StockQuantity { get; set; }
    public string? Emoji { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
