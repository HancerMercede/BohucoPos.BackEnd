using NexusPOS.Domain.Enums;

namespace NexusPOS.Application.DTOs;

public record CloseTabDto(PaymentMethod PaymentMethod, bool DirectClose = false);