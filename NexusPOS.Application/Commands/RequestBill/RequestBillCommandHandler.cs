using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Commands.RequestBill;

public class RequestBillCommandHandler(IUnitOfWork uow) : IRequestHandler<RequestBillCommand, Unit>
{
    public async Task<Unit> Handle(RequestBillCommand request, CancellationToken ct)
    {
        var tabEntity = await uow.Tabs.GetByIdAsync(request.TabId, ct);
        if (tabEntity is null)
            throw new InvalidOperationException("Tab not found");

        var tab = await uow.Tabs.GetWithOrdersAsync(request.TabId, ct);
        if (tab is null)
            throw new InvalidOperationException("Tab not found");

        tab.RequestBill();

        tabEntity.Status = tab.Status;
        uow.Tabs.Update(tabEntity);
        await uow.CommitAsync(ct);

        return Unit.Value;
    }
}
