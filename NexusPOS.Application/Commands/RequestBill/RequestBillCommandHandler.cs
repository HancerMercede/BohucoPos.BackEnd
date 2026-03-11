using MediatR;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;

namespace NexusPOS.Application.Commands.RequestBill;

public class RequestBillCommandHandler(ITabRepository tabRepository) : IRequestHandler<RequestBillCommand, Unit>
{
    public async Task<Unit> Handle(RequestBillCommand request, CancellationToken ct)
    {
        var tabEntity = await tabRepository.GetByIdAsync(request.TabId, ct);
        if (tabEntity == null)
            throw new InvalidOperationException("Tab not found");

        var tab = await tabRepository.GetWithOrdersAsync(request.TabId, ct);
        if (tab == null)
            throw new InvalidOperationException("Tab not found");

        tab.RequestBill();

        // Update the entity
        tabEntity.Status = tab.Status;
        tabRepository.Update(tabEntity);
        await tabRepository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
