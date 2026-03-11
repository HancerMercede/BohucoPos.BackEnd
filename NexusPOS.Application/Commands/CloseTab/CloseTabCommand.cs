using MediatR;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.CloseTab;

public record CloseTabCommand : IRequest<Unit>
{
    public int TabId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public bool DirectClose { get; init; } = false;
}
