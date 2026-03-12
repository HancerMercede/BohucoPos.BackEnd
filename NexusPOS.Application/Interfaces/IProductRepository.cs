using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Interfaces;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByDestinationAsync(ItemDestination destination, CancellationToken ct = default);
}
