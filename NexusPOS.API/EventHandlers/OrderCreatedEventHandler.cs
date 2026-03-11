using MediatR;
using Microsoft.AspNetCore.SignalR;
using NexusPOS.Application.Events;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Queries.GetPendingOrdersByDestination;
using NexusPOS.Application.Hubs;
using NexusPOS.Domain.Enums;

namespace NexusPOS.API.EventHandlers;

public class OrderCreatedEventHandler(IHubContext<OrderHub> hub, IMediator mediator)
    : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken ct)
    {
        var allOrders = await mediator.Send(new GetPendingOrdersByDestinationQuery { Destination = ItemDestination.Kitchen }, ct);
        
        var orderData = allOrders.FirstOrDefault(o => o.Id == notification.OrderId);
        if (orderData is null) return;

        var kitchenItems = orderData.Items.Where(i => i.Destination == ItemDestination.Kitchen);
        var barItems = orderData.Items.Where(i => i.Destination == ItemDestination.Bar);

        if (kitchenItems.Any())
            await hub.Clients.Group("kitchen").SendAsync("NewOrder", kitchenItems, ct);

        if (barItems.Any())
            await hub.Clients.Group("bar").SendAsync("NewOrder", barItems, ct);
    }
}
