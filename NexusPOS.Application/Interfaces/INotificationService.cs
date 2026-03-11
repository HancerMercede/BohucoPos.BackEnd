namespace NexusPOS.Application.Interfaces;

public interface INotificationService
{
    Task SendToWaiterAsync(Guid waiterId, string method, object? data, CancellationToken ct = default);
}
