using MediatR;
using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Queries.GetLowInventory;

public record GetLowInventoryQuery(int? Threshold = 10) : IRequest<List<ProductDto>>;

