using Microsoft.EntityFrameworkCore.Storage;
using NexusPOS.Application.Interfaces;
using NexusPOS.Infrastructure.Data;
using NexusPOS.Infrastructure.Repositories;

namespace NexusPOS.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private readonly Lazy<IOrderRepository> _orders;
    private readonly Lazy<IOrderItemRepository> _orderItems;
    private readonly Lazy<ITabRepository> _tabs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        
        _orders = new Lazy<IOrderRepository>(() => new OrderRepository(_context));
        _orderItems = new Lazy<IOrderItemRepository>(() => new OrderItemRepository(_context));
        _tabs = new Lazy<ITabRepository>(() => new TabRepository(_context));
    }

    public IOrderRepository Orders => _orders.Value;
    public IOrderItemRepository OrderItems => _orderItems.Value;
    public ITabRepository Tabs => _tabs.Value;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        var result = await _context.SaveChangesAsync(ct);
        
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        
        return result;
    }

    public async Task RollbackAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
