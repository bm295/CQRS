using WebApplication.Application.Queries;

namespace WebApplication.ViewModels;

public sealed class DashboardPageVm
{
    public required OpsSummaryVm Summary { get; init; }
    public required IReadOnlyList<PendingApprovalVm> PendingApprovals { get; init; }
    public required IReadOnlyList<ScheduledChangeVm> ScheduledChanges { get; init; }
    public required IReadOnlyList<FailedChangeVm> FailedChanges { get; init; }
}
