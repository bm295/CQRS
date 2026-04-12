using WebApplication.Application.Queries;
using WebApplication.Domain;

namespace WebApplication.Infrastructure.ReadModels;

public interface IChangeReadModelStore
{
    Task UpsertAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default);
    Task<ChangeRequestDetailsVm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PendingApprovalVm>> ListPendingApprovalsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduledChangeVm>> ListScheduledChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FailedChangeVm>> ListFailedChangesAsync(CancellationToken cancellationToken = default);
    Task<OpsSummaryVm> GetSummaryAsync(CancellationToken cancellationToken = default);
}
