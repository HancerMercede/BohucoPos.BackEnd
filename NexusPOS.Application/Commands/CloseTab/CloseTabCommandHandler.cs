using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Commands.CloseTab;

public class CloseTabCommandHandler(IUnitOfWork uow) : IRequestHandler<CloseTabCommand, Unit>
{
    public async Task<Unit> Handle(CloseTabCommand request, CancellationToken ct)
    {
        var tabEntity = await uow.Tabs.GetByIdAsync(request.TabId, ct);
        if (tabEntity == null)
            throw new InvalidOperationException("Tab not found");

        var tab = await uow.Tabs.GetWithOrdersAsync(request.TabId, ct);
        if (tab == null)
            throw new InvalidOperationException("Tab not found");

        if (request.DirectClose)
        {
            tab.CloseDirect(request.PaymentMethod);
        }
        else
        {
            tab.Close(request.PaymentMethod);
        }

        tabEntity.Status = tab.Status;
        tabEntity.ClosedAt = tab.ClosedAt;
        tabEntity.PaymentMethod = tab.PaymentMethod;

        uow.Tabs.Update(tabEntity);
        await uow.CommitAsync(ct);

        return Unit.Value;
    }
}
