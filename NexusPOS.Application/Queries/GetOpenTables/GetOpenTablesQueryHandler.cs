using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetOpenTables;

public class GetOpenTablesQueryHandler(IUnitOfWork uow) 
    : IRequestHandler<GetOpenTablesQuery, IEnumerable<OpenTableDto>>
{
    public async Task<IEnumerable<OpenTableDto>> Handle(GetOpenTablesQuery request, CancellationToken ct)
    {
        var tabs = await uow.Tabs.GetOpenTabsAsync(ct);
        
        return tabs
            .GroupBy(t => t.Location)
            .Select(g => new OpenTableDto(g.Key, g.Count()));
    }
}
