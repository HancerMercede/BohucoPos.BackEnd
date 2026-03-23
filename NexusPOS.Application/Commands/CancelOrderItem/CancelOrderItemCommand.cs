using MediatR;

namespace NexusPOS.Application.Commands.CancelOrderItem;

public record CancelOrderItemCommand(int OrderItemId, string? Reason = null) : IRequest<Unit>;
