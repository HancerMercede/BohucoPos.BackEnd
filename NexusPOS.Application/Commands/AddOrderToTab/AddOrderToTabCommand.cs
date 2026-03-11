using MediatR;

namespace NexusPOS.Application.Commands.AddOrderToTab;

public record AddOrderToTabCommand : IRequest<Unit>
{
    public int TabId { get; init; }
    public int OrderId { get; init; }
}
