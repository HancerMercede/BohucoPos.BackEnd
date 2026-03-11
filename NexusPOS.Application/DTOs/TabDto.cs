using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.DTOs;

public record TabDto
{
    public int Id { get; init; }
    public string IdDisplay { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string WaiterName { get; init; } = string.Empty;
    public TabStatus Status { get; init; }
    public DateTime OpenedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public string? Notes { get; init; }
    public List<OrderDto> Orders { get; init; } = new();
}
