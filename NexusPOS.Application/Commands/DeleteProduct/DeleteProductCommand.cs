using MediatR;
using NexusPOS.Application.Interfaces;

namespace NexusPOS.Application.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<Unit>;

