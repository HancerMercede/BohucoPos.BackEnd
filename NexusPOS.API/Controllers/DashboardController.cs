using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Queries.GetSalesAnalytics;
using NexusPOS.Application.Queries.GetLowInventory;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("sales")]
    public async Task<SalesAnalyticsDto> GetSalesAnalytics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
        => await mediator.Send(new GetSalesAnalyticsQuery(startDate, endDate), ct);

    [HttpGet("low-inventory")]
    public async Task<List<ProductDto>> GetLowInventory(CancellationToken ct,[FromQuery] int threshold)
        => await mediator.Send(new GetLowInventoryQuery(threshold), ct);
}
