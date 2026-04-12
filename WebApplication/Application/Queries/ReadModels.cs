using WebApplication.Domain;

namespace WebApplication.Application.Queries;

public sealed record ChangeRequestDetailsVm(
    Guid Id,
    string Title,
    string SystemName,
    string Environment,
    string RequestedBy,
    string? ApprovedBy,
    DateTime? ScheduledAtUtc,
    ChangeStatus Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record PendingApprovalVm(
    Guid Id,
    string Title,
    string SystemName,
    string Environment,
    string RequestedBy,
    DateTime CreatedAtUtc);

public sealed record ScheduledChangeVm(
    Guid Id,
    string Title,
    string SystemName,
    string Environment,
    DateTime ScheduledAtUtc,
    string ApprovedBy);

public sealed record FailedChangeVm(
    Guid Id,
    string Title,
    string SystemName,
    string Environment,
    string FailureReason,
    DateTime UpdatedAtUtc);

public sealed record OpsSummaryVm(
    int PendingApprovals,
    int ScheduledChanges,
    int FailedChanges,
    int InProgressChanges,
    DateTime LastProjectedAtUtc);
