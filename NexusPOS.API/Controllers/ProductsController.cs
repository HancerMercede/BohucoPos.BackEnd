using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Commands.CreateProduct;
using NexusPOS.Application.Commands.DeleteProduct;
using NexusPOS.Application.Commands.UpdateProduct;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Queries.GetProducts;
using NexusPOS.Domain.Enums;

namespace NexusPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ProductDto>> GetAll([FromQuery] ItemDestination? destination, CancellationToken ct)
        => await mediator.Send(new GetProductsQuery(destination), ct);

    [HttpGet("{id:int}")]
    public async Task<ProductDto> GetById(int id, CancellationToken ct)
        => await mediator.Send(new GetProductByIdQuery(id), ct);

    [HttpPost]
    public async Task<ProductDto> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        => await mediator.Send(new CreateProductCommand(dto), ct);

    [HttpPut("{id:int}")]
    public async Task<ProductDto> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        => await mediator.Send(new UpdateProductCommand(id, dto), ct);

    [HttpDelete("{id:int}")]
    public async Task<Unit> Delete(int id, CancellationToken ct)
        => await mediator.Send(new DeleteProductCommand(id), ct);
}
