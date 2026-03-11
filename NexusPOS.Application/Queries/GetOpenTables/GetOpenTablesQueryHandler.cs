using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetOpenTables;

public class GetOpenTablesQueryHandler(ITabRepository tabRepository) 
    : IRequestHandler<GetOpenTablesQuery, IEnumerable<OpenTableDto>>
{
    public async Task<IEnumerable<OpenTableDto>> Handle(GetOpenTablesQuery request, CancellationToken ct)
    {
        var tabs = await tabRepository.GetOpenTabsAsync(ct);
        
        return tabs
            .GroupBy(t => t.Location)
            .Select(g => new OpenTableDto(g.Key, g.Count()));
    }
}
