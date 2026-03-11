using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Commands.AddOrderToTab;
using NexusPOS.Application.Commands.CancelTab;
using NexusPOS.Application.Commands.CloseTab;
using NexusPOS.Application.Commands.OpenTab;
using NexusPOS.Application.Commands.RequestBill;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Application.Queries.GetActiveTabsByLocation;
using NexusPOS.Application.Queries.GetOpenTables;
using NexusPOS.Application.Queries.GetTabDetails;
using NexusPOS.Application.Services;
using NexusPOS.Domain.Enums;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TabsController(IMediator mediator, IPdfService pdfService) : ControllerBase
{
    [HttpPost]
    public async Task<int> OpenTab([FromBody] OpenTabCommand command, CancellationToken ct)
        => await mediator.Send(command, ct);

    [HttpGet("location/{location}")]
    public async Task<IEnumerable<TabDto>> GetActiveByLocation(string location, CancellationToken ct)
        => await mediator.Send(new GetActiveTabsByLocationQuery(location), ct);

    [HttpGet("active")]
    public async Task<IEnumerable<TabDto>> GetAllActiveTabs(CancellationToken ct)
        => await mediator.Send(new GetActiveTabsByLocationQuery(null!), ct);

    [HttpGet("{tabId:int}")]
    public async Task<TabDto?> GetTabDetails(int tabId, CancellationToken ct)
        => await mediator.Send(new GetTabDetailsQuery(tabId), ct);

    [HttpGet("open")]
    public async Task<IEnumerable<OpenTableDto>> GetOpenTables(CancellationToken ct)
        => await mediator.Send(new GetOpenTablesQuery(), ct);

    [HttpPost("orders")]
    public async Task<Unit> AddOrderToTab([FromBody] AddOrderToTabCommand command, CancellationToken ct)
        => await mediator.Send(command, ct);

    [HttpPost("{tabId:int}/request-bill")]
    public async Task<Unit> RequestBill(int tabId, CancellationToken ct)
        => await mediator.Send(new RequestBillCommand { TabId = tabId }, ct);

    [HttpPost("{tabId:int}/close")]
    public async Task<Unit> CloseTab(int tabId, [FromBody] CloseTabDto dto, CancellationToken ct)
        => await mediator.Send(new CloseTabCommand { TabId = tabId, PaymentMethod = dto.PaymentMethod, DirectClose = dto.DirectClose }, ct);

    [HttpPost("{tabId:int}/cancel")]
    public async Task<Unit> CancelTab(int tabId, [FromBody] CancelTabDto? dto, CancellationToken ct)
        => await mediator.Send(new CancelTabCommand { TabId = tabId, Reason = dto?.Reason }, ct);

    [HttpGet("{tabId:int}/pdf")]
    public async Task<IActionResult> GetBillPdf(int tabId, CancellationToken ct)
    {
        var tabDto = await mediator.Send(new GetTabDetailsQuery(tabId), ct);
        if (tabDto == null) return NotFound();

        var billData = new TabBillData
        {
            TabId = tabDto.IdDisplay,
            CustomerName = tabDto.CustomerName,
            Location = tabDto.Location,
            WaiterName = tabDto.WaiterName ?? "Mesero",
            OpenedAt = tabDto.OpenedAt,
            RequestedAt = DateTime.UtcNow,
            Items = tabDto.Orders.SelectMany(o => o.Items).Select(i => new BillItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Notes = i.Notes
            }).ToList()
        };

        billData.Subtotal = billData.Items.Sum(i => i.Total);
        billData.Tax = billData.Subtotal * 0.18m;
        billData.Total = billData.Subtotal + billData.Tax;

        var pdfBytes = pdfService.GenerateBillPdf(billData);
        return File(pdfBytes, "application/pdf", "cuenta_" + billData.Location + ".pdf");
    }
}

public record CloseTabDto(PaymentMethod PaymentMethod, bool DirectClose = false);
public record CancelTabDto(string? Reason = null);
