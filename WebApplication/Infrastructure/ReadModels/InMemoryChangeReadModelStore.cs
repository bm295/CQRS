using System.Collections.Concurrent;
using WebApplication.Application.Queries;
using WebApplication.Domain;

namespace WebApplication.Infrastructure.ReadModels;

public sealed class InMemoryChangeReadModelStore : IChangeReadModelStore
{
    private readonly ConcurrentDictionary<Guid, ChangeRequestDetailsVm> _details = new();
    private DateTime _lastProjectedAtUtc = DateTime.UtcNow;

    public Task UpsertAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default)
    {
        var details = new ChangeRequestDetailsVm(
            changeRequest.Id,
            changeRequest.Title,
            changeRequest.SystemName,
            changeRequest.Environment,
            changeRequest.RequestedBy,
            changeRequest.ApprovedBy,
            changeRequest.ScheduledAtUtc,
            changeRequest.Status,
            changeRequest.FailureReason,
            changeRequest.CreatedAtUtc,
            changeRequest.UpdatedAtUtc);

        _details[changeRequest.Id] = details;
        _lastProjectedAtUtc = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task<ChangeRequestDetailsVm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _details.TryGetValue(id, out var model);
        return Task.FromResult(model);
    }

    public Task<IReadOnlyList<PendingApprovalVm>> ListPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var results = _details.Values
            .Where(x => x.Status == ChangeStatus.PendingApproval)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new PendingApprovalVm(x.Id, x.Title, x.SystemName, x.Environment, x.RequestedBy, x.CreatedAtUtc))
            .ToList();

        return Task.FromResult<IReadOnlyList<PendingApprovalVm>>(results);
    }

    public Task<IReadOnlyList<ScheduledChangeVm>> ListScheduledChangesAsync(CancellationToken cancellationToken = default)
    {
        var results = _details.Values
            .Where(x => x.Status == ChangeStatus.Scheduled && x.ScheduledAtUtc.HasValue && !string.IsNullOrWhiteSpace(x.ApprovedBy))
            .OrderBy(x => x.ScheduledAtUtc)
            .Select(x => new ScheduledChangeVm(x.Id, x.Title, x.SystemName, x.Environment, x.ScheduledAtUtc!.Value, x.ApprovedBy!))
            .ToList();

        return Task.FromResult<IReadOnlyList<ScheduledChangeVm>>(results);
    }

    public Task<IReadOnlyList<FailedChangeVm>> ListFailedChangesAsync(CancellationToken cancellationToken = default)
    {
        var results = _details.Values
            .Where(x => x.Status == ChangeStatus.Failed && !string.IsNullOrWhiteSpace(x.FailureReason))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new FailedChangeVm(x.Id, x.Title, x.SystemName, x.Environment, x.FailureReason!, x.UpdatedAtUtc))
            .ToList();

        return Task.FromResult<IReadOnlyList<FailedChangeVm>>(results);
    }

    public Task<OpsSummaryVm> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var values = _details.Values;
        var summary = new OpsSummaryVm(
            values.Count(x => x.Status == ChangeStatus.PendingApproval),
            values.Count(x => x.Status == ChangeStatus.Scheduled),
            values.Count(x => x.Status == ChangeStatus.Failed),
            values.Count(x => x.Status == ChangeStatus.InProgress),
            _lastProjectedAtUtc);

        return Task.FromResult(summary);
    }
}
