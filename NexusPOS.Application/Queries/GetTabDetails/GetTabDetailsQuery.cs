using MediatR;
using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Queries.GetTabDetails;

public record GetTabDetailsQuery(int TabId) : IRequest<TabDto?>;
