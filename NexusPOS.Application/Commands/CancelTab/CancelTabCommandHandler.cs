using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Commands.CancelTab;

public class CancelTabCommandHandler(IUnitOfWork uow) : IRequestHandler<CancelTabCommand, Unit>
{
    public async Task<Unit> Handle(CancelTabCommand request, CancellationToken ct)
    {
        var tabEntity = await uow.Tabs.GetByIdAsync(request.TabId, ct);
        if (tabEntity == null)
            throw new InvalidOperationException("Tab not found");

        var tab = await uow.Tabs.GetWithOrdersAsync(request.TabId, ct);
        if (tab == null)
            throw new InvalidOperationException("Tab not found");

        tab.Cancel(request.Reason);

        tabEntity.Status = tab.Status;
        tabEntity.ClosedAt = tab.ClosedAt;
        tabEntity.Notes = tab.Notes;

        uow.Tabs.Update(tabEntity);
        await uow.CommitAsync(ct);

        return Unit.Value;
    }
}
