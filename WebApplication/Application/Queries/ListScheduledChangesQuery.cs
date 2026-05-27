using MediatR;

namespace WebApplication.Application.Queries;

public sealed record ListScheduledChangesQuery : IRequest<IReadOnlyList<ScheduledChangeVm>>;
