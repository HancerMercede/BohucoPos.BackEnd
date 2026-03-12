using Microsoft.EntityFrameworkCore;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;
using NexusPOS.Infrastructure.Data;

namespace NexusPOS.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : RepositoryBase<Product>(context), IProductRepository
{
    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct = default)
        => await context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<IEnumerable<Product>> GetByDestinationAsync(ItemDestination destination, CancellationToken ct = default)
        => await context.Products
            .Where(p => p.IsActive && p.Destination == destination)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);
}
