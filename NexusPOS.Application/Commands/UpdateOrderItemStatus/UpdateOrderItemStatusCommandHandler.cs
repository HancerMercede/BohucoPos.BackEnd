using MediatR;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Commands.UpdateOrderItemStatus;
using Microsoft.Extensions.Logging;

namespace NexusPOS.Application.Commands.UpdateOrderItemStatus;

public class UpdateOrderItemStatusCommandHandler(
    IUnitOfWork uow,
    INotificationService notificationService,
    ILogger<UpdateOrderItemStatusCommandHandler> logger)
    : IRequestHandler<UpdateOrderItemStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateOrderItemStatusCommand cmd, CancellationToken ct)
    {
        try
        {
            await uow.BeginTransactionAsync(ct);

            var item = await uow.OrderItems.GetByIdAsync(cmd.OrderItemId, ct)
                ?? throw new KeyNotFoundException($"OrderItem {cmd.OrderItemId} not found");

            item.Status = cmd.NewStatus;
            uow.OrderItems.Update(item);
            await uow.CommitAsync(ct);

            logger.LogInformation("OrderItem {ItemId} status updated to {Status}", item.Id, cmd.NewStatus);

            var order = await uow.Orders.GetWithItemsAsync(item.OrderId, ct);
            if (order?.Tab != null && !string.IsNullOrEmpty(order.Tab.WaiterId))
            {
                if (Guid.TryParse(order.Tab.WaiterId, out var waiterId))
                {
                    await notificationService.SendToWaiterAsync(
                        waiterId,
                        "OrderItemStatusChanged",
                        new { ItemId = item.Id, Status = cmd.NewStatus.ToString(), ItemName = item.ProductName },
                        ct
                    );
                }
            }

            return Unit.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating order item status, rolling back");
            await uow.RollbackAsync();
            throw;
        }
    }
}
