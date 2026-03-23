using MediatR;
using NexusPOS.Application.DTOs;

namespace NexusPOS.Application.Queries.GetSalesAnalytics;

public record GetSalesAnalyticsQuery(DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<SalesAnalyticsDto>;


