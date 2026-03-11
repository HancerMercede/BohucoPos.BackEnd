using MediatR;

namespace NexusPOS.Application.Commands.CancelTab;

public record CancelTabCommand : IRequest<Unit>
{
    public int TabId { get; init; }
    public string? Reason { get; init; }
}
