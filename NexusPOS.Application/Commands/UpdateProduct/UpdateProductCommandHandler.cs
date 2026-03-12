using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(request.Id, ct)
                      ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        product.Name = request.Dto.Name;
        product.Price = request.Dto.Price;
        product.Category = request.Dto.Category;
        product.Destination = request.Dto.Destination;
        product.ProductType = request.Dto.ProductType;
        product.Emoji = request.Dto.Emoji;
        
        if (request.Dto.ProductType == ProductType.Physical)
            product.StockQuantity = request.Dto.StockQuantity;
        else
            product.StockQuantity = null;

        uow.Products.Update(product);
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