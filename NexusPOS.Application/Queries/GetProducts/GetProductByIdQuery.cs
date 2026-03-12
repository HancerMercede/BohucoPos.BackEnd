using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetProducts;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Category = product.Category,
            Destination = product.Destination,
            ProductType = product.ProductType,
            StockQuantity = product.StockQuantity,
            Emoji = product.Emoji,
            IsActive = product.IsActive
        };
    }
}
