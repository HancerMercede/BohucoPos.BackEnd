using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.UpdateProduct;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<ProductDto>;


