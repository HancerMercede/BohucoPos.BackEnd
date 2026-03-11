using MediatR;

namespace NexusPOS.Application.Queries.GetOpenTables;

public record GetOpenTablesQuery : IRequest<IEnumerable<OpenTableDto>>;

public record OpenTableDto(string Location, int TabCount);
