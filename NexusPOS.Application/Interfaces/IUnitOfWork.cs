namespace NexusPOS.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    ITabRepository Tabs { get; }

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<int> CommitAsync(CancellationToken ct = default);
    Task RollbackAsync();
}
