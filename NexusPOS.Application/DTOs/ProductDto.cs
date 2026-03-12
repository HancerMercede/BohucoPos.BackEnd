using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.DTOs;

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public ItemDestination Destination { get; init; }
    public ProductType ProductType { get; init; }
    public int? StockQuantity { get; init; }
    public string? Emoji { get; init; }
    public bool IsActive { get; init; }
}

public record CreateProductDto(
    string Name,
    decimal Price,
    string Category,
    ItemDestination Destination,
    ProductType ProductType,
    int? StockQuantity,
    string? Emoji
);

public record UpdateProductDto(
    string Name,
    decimal Price,
    string Category,
    ItemDestination Destination,
    ProductType ProductType,
    int? StockQuantity,
    string? Emoji
);
