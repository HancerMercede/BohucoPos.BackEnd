using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Infrastructure.Data;

namespace NexusPOS.Infrastructure.Repositories;

public class OrderItemRepository(AppDbContext context) : RepositoryBase<OrderItem>(context), IOrderItemRepository { }
