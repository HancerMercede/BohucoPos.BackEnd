using MediatR;
using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Queries.GetActiveTabsByLocation;

public record GetActiveTabsByLocationQuery(string Location) : IRequest<IEnumerable<TabDto>>;
