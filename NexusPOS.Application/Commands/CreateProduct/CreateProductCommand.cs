using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;

public class CreateProductCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Dto.Name,
            Price = request.Dto.Price,
            Category = request.Dto.Category,
            Destination = request.Dto.Destination,
            ProductType = request.Dto.ProductType,
            StockQuantity = request.Dto.ProductType == ProductType.Physical ? request.Dto.StockQuantity : null,
            Emoji = request.Dto.Emoji,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.Products.AddAsync(product, ct);
        await uow.CommitAsync(ct);

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
