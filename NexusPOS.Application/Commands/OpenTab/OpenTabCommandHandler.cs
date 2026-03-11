using MediatR;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;

namespace NexusPOS.Application.Commands.OpenTab;

public class OpenTabCommandHandler(IUnitOfWork uow) : IRequestHandler<OpenTabCommand, int>
{
    public async Task<int> Handle(OpenTabCommand request, CancellationToken ct)
    {
        var tab = Tab.Open(
            request.Location,
            request.CustomerName,
            request.WaiterId,
            request.WaiterName,
            request.TaxRate
        );

        await uow.Tabs.AddAsync(tab, ct);
        await uow.CommitAsync(ct);

        return tab.Id;
    }
}
