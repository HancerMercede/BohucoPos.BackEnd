using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Queries.GetLowInventory;

public class GetLowInventoryQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetLowInventoryQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetLowInventoryQuery request, CancellationToken ct)
    {
        var products = await unitOfWork.Products.GetAllAsync(ct);
        
        return products
            .Where(p => p.ProductType == Domain.Enums.ProductType.Physical && 
                       p.StockQuantity.HasValue && 
                       p.StockQuantity.Value <= request.Threshold)
            .Select(p => new ProductDto
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
            })
            .OrderBy(p => p.StockQuantity)
            .ToList();
    }
}
