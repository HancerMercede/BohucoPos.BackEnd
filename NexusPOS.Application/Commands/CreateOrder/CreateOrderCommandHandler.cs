using MediatR;
using NexusPOS.Application.Commands.CreateOrder;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Events;
using NexusPOS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NexusPOS.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IUnitOfWork uow,
    IOrderRoutingService routing,
    IPublisher publisher,
    ILogger<CreateOrderCommandHandler> logger)
    : IRequestHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        try
        {
            await uow.BeginTransactionAsync(ct);

            var order = Order.Create(cmd.OrderType, cmd.TableId, cmd.WaiterName);

            foreach (var item in cmd.Items)
            {
                var destination = await routing.ResolveDestinationAsync(item.ProductId, ct);
                order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.Notes, destination);
            }

            await uow.Orders.AddAsync(order, ct);
            await uow.CommitAsync(ct);

            await publisher.Publish(new OrderCreatedEvent(order.Id), ct);

            logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return order.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order, rolling back transaction");
            await uow.RollbackAsync();
            throw;
        }
    }
}
