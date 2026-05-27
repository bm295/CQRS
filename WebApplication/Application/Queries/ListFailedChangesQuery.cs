using MediatR;

namespace WebApplication.Application.Queries;

public sealed record ListFailedChangesQuery : IRequest<IReadOnlyList<FailedChangeVm>>;
