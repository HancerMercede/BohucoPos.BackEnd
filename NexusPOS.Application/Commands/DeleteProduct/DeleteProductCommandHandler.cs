using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await uow.Products.GetByIdAsync(request.Id, ct)
                      ?? throw new KeyNotFoundException($"Product {request.Id} not found");

        product.IsActive = false;
        uow.Products.Update(product);
        await uow.CommitAsync(ct);

        return Unit.Value;
    }
}
