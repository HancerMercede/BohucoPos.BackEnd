using MediatR;
using NexusPOS.Application.Commands.CancelOrderItem;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.CancelOrderItem;

public class CancelOrderItemCommandHandler(IUnitOfWork uow) 
    : IRequestHandler<CancelOrderItemCommand, Unit>
{
    public async Task<Unit> Handle(CancelOrderItemCommand request, CancellationToken ct)
    {
        var item = await uow.OrderItems.GetByIdAsync(request.OrderItemId, ct);
        
        if (item == null)
            throw new KeyNotFoundException($"Order item {request.OrderItemId} not found");
        
        if (item.Status == ItemStatus.Cancelled)
            throw new InvalidOperationException("Item is already cancelled");
        
        item.Status = ItemStatus.Cancelled;
        uow.OrderItems.Update(item);
        await uow.CommitAsync(ct);
        
        return Unit.Value;
    }
}
