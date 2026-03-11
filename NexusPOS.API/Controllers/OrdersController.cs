using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Commands.CancelOrderItem;
using NexusPOS.Application.Commands.CreateOrder;
using NexusPOS.Application.Commands.UpdateOrderItemStatus;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Queries.GetOrdersByTable;
using NexusPOS.Application.Queries.GetPendingOrdersByDestination;
using NexusPOS.Domain.Enums;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<int> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken ct)
        => await mediator.Send(command, ct);

    [HttpGet("table/{tableId}")]
    public async Task<IEnumerable<OrderDto>> GetByTable(string tableId, CancellationToken ct)
        => await mediator.Send(new GetOrdersByTableQuery { TableId = tableId }, ct);

    [HttpGet("pending/{destination}")]
    public async Task<IEnumerable<OrderDto>> GetPending(int destination, CancellationToken ct)
        => await mediator.Send(new GetPendingOrdersByDestinationQuery { Destination = (ItemDestination)destination }, ct);

    [HttpPatch("items/{itemId:int}/status")]
    public async Task<Unit> UpdateItemStatus(int itemId, [FromBody] UpdateStatusDto dto, CancellationToken ct)
        => await mediator.Send(new UpdateOrderItemStatusCommand 
        { 
            OrderItemId = itemId, 
            NewStatus = dto.Status 
        }, ct);

    [HttpDelete("items/{itemId:int}")]
    public async Task<Unit> CancelItem(int itemId, [FromBody] CancelItemDto? dto, CancellationToken ct)
        => await mediator.Send(new CancelOrderItemCommand(itemId, dto?.Reason), ct);
}

public record UpdateStatusDto(ItemStatus Status);
public record CancelItemDto(string? Reason = null);
