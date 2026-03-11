using MediatR;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.UpdateOrderItemStatus;

public record UpdateOrderItemStatusCommand : IRequest<Unit>
{
    public int OrderItemId { get; init; }
    public ItemStatus NewStatus { get; init; }
}
