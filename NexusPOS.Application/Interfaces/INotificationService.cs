namespace NexusPOS.Application.Interfaces;

public interface INotificationService
{
    Task SendToWaiterAsync(string waiterUsername, string method, object? data, CancellationToken ct = default);
}
