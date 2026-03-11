using MediatR;

namespace NexusPOS.Application.Events;

public record OrderCreatedEvent(int OrderId) : INotification;
