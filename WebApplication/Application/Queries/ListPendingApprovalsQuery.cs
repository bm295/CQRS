using MediatR;

namespace WebApplication.Application.Queries;

public sealed record ListPendingApprovalsQuery : IRequest<IReadOnlyList<PendingApprovalVm>>;
