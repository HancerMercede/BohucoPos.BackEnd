using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Interfaces;

public interface IOrderRoutingService
{
    Task<ItemDestination> ResolveDestinationAsync(string productId, CancellationToken ct = default);
}
