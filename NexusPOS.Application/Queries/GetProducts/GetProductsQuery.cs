using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Queries.GetProducts;

public record GetProductsQuery(ItemDestination? Destination = null) : IRequest<IEnumerable<ProductDto>>;

public class GetProductsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var products = request.Destination.HasValue
            ? await uow.Products.GetByDestinationAsync(request.Destination.Value, ct)
            : await uow.Products.GetActiveAsync(ct);

        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Category = p.Category,
            Destination = p.Destination,
            ProductType = p.ProductType,
            StockQuantity = p.StockQuantity,
            Emoji = p.Emoji,
            IsActive = p.IsActive
        });
    }
}
