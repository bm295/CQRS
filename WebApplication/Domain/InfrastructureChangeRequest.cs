using Cqrs.RetailerIsolation;

namespace WebApplication.Domain;

public sealed class InfrastructureChangeRequest : IBelongsToRetailer
{
    private InfrastructureChangeRequest(
        Guid id,
        string title,
        string systemName,
        string environment,
        string requestedBy,
        DateTime createdAtUtc)
    {
        Id = id;
        Title = title;
        SystemName = systemName;
        Environment = environment;
        RequestedBy = requestedBy;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Status = ChangeStatus.Draft;
    }

    public Guid Id { get; }
    public Guid RetailerId { get; private set; }
    public string Title { get; private set; }
    public string SystemName { get; private set; }
    public string Environment { get; private set; }
    public string RequestedBy { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ScheduledAtUtc { get; private set; }
    public ChangeStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static InfrastructureChangeRequest Create(
        string title,
        string systemName,
        string environment,
        string requestedBy,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainRuleViolationException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(systemName))
        {
            throw new DomainRuleViolationException("System name is required.");
        }

        if (string.IsNullOrWhiteSpace(requestedBy))
        {
            throw new DomainRuleViolationException("Requester is required.");
        }

        var normalizedEnvironment = NormalizeEnvironment(environment);
        return new InfrastructureChangeRequest(Guid.NewGuid(), title.Trim(), systemName.Trim(), normalizedEnvironment, requestedBy.Trim(), utcNow);
    }

    public void SubmitForApproval(DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.Draft, "Only draft requests can be submitted.");
        FailureReason = null;
        Touch(utcNow);
        Status = ChangeStatus.PendingApproval;
    }

    public void Approve(string approvedBy, DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.PendingApproval, "Only pending requests can be approved.");

        if (string.IsNullOrWhiteSpace(approvedBy))
        {
            throw new DomainRuleViolationException("Approver is required.");
        }

        ApprovedBy = approvedBy.Trim();
        FailureReason = null;
        Touch(utcNow);
        Status = ChangeStatus.Approved;
    }

    public void Reject(string approvedBy, string reason, DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.PendingApproval, "Only pending requests can be rejected.");

        if (string.IsNullOrWhiteSpace(approvedBy))
        {
            throw new DomainRuleViolationException("Approver is required.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleViolationException("Rejection reason is required.");
        }

        ApprovedBy = approvedBy.Trim();
        FailureReason = reason.Trim();
        Touch(utcNow);
        Status = ChangeStatus.Rejected;
    }

    public void ScheduleExecution(DateTime scheduledAtUtc, DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.Approved, "Only approved requests can be scheduled.");

        if (scheduledAtUtc <= utcNow.AddMinutes(-1))
        {
            throw new DomainRuleViolationException("Scheduled time must be in the future.");
        }

        ScheduledAtUtc = DateTime.SpecifyKind(scheduledAtUtc, DateTimeKind.Utc);
        FailureReason = null;
        Touch(utcNow);
        Status = ChangeStatus.Scheduled;
    }

    public void MarkExecutionStarted(DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.Scheduled, "Only scheduled requests can start execution.");
        FailureReason = null;
        Touch(utcNow);
        Status = ChangeStatus.InProgress;
    }

    public void MarkExecutionCompleted(DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.InProgress, "Only in-progress requests can complete.");
        FailureReason = null;
        Touch(utcNow);
        Status = ChangeStatus.Completed;
    }

    public void MarkExecutionFailed(string failureReason, DateTime utcNow)
    {
        EnsureStatus(ChangeStatus.InProgress, "Only in-progress requests can fail.");

        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new DomainRuleViolationException("Failure reason is required.");
        }

        FailureReason = failureReason.Trim();
        Touch(utcNow);
        Status = ChangeStatus.Failed;
    }

    private static string NormalizeEnvironment(string environment)
    {
        var normalized = environment?.Trim().ToLowerInvariant();
        return normalized switch
        {
            "dev" or "staging" or "prod" => normalized,
            _ => throw new DomainRuleViolationException("Environment must be one of: dev, staging, prod.")
        };
    }

    private void EnsureStatus(ChangeStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new DomainRuleViolationException(message);
        }
    }

    private void Touch(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
    }
}
