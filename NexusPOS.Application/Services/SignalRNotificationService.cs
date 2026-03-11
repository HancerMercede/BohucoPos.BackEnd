using Microsoft.AspNetCore.SignalR;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Hubs;

namespace NexusPOS.Application.Services;

public class SignalRNotificationService(IHubContext<OrderHub> hub) : INotificationService
{
    public async Task SendToWaiterAsync(Guid waiterId, string method, object? data, CancellationToken ct = default)
    {
        await hub.Clients.Group($"waiter-{waiterId}").SendAsync(method, data, ct);
    }
}
