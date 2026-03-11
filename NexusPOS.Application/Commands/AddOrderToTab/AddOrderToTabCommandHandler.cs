using MediatR;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.AddOrderToTab;

public class AddOrderToTabCommandHandler(IUnitOfWork uow) 
    : IRequestHandler<AddOrderToTabCommand, Unit>
{
    public async Task<Unit> Handle(AddOrderToTabCommand request, CancellationToken ct)
    {
        var tab = await uow.Tabs.GetWithOrdersAsync(request.TabId, ct);
        if (tab == null)
            throw new InvalidOperationException("Tab not found");

        if (tab.Status != TabStatus.Open)
            throw new InvalidOperationException("Cannot add order to a tab that is not open");

        var orderEntity = await uow.Orders.GetByIdAsync(request.OrderId, ct);
        if (orderEntity == null)
            throw new InvalidOperationException("Order not found");

        orderEntity.TabId = request.TabId;
        uow.Orders.Update(orderEntity);
        await uow.CommitAsync(ct);

        return Unit.Value;
    }
}
