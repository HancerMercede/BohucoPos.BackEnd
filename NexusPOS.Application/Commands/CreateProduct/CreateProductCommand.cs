using MediatR;
using NexusPOS.Application.DTOs;
using NexusPOS.Application.Interfaces;
using NexusPOS.Domain.Entities;
using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;


