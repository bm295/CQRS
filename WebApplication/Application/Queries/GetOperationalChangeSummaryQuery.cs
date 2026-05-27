using MediatR;

namespace WebApplication.Application.Queries;

public sealed record GetOperationalChangeSummaryQuery : IRequest<OpsSummaryVm>;
