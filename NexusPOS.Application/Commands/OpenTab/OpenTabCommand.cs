using MediatR;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.OpenTab;

public record OpenTabCommand : IRequest<int>
{
    public string Location { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string WaiterId { get; init; } = string.Empty;
    public string WaiterName { get; init; } = string.Empty;
    public decimal TaxRate { get; init; } = 0.18m;
}
