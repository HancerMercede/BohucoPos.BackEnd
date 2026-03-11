using MediatR;

namespace NexusPOS.Application.Commands.RequestBill;

public record RequestBillCommand : IRequest<Unit>
{
    public int TabId { get; init; }
}
