using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Services;

public class OrderRoutingService : IOrderRoutingService
{
    private static readonly string[] BarProducts = { "p7", "p8", "p9", "p10", "p11" };

    public Task<ItemDestination> ResolveDestinationAsync(string productId, CancellationToken ct = default)
    {
        var destination = BarProducts.Contains(productId) ? ItemDestination.Bar : ItemDestination.Kitchen;
        return Task.FromResult(destination);
    }
}
