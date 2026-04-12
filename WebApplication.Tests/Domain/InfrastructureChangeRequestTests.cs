using NUnit.Framework;
using WebApplication.Domain;

namespace WebApplication.Tests.Domain;

[TestFixture]
public sealed class InfrastructureChangeRequestTests
{
    [Test]
    public void Full_success_path_applies_expected_status_transitions()
    {
        var now = new DateTime(2026, 4, 12, 7, 0, 0, DateTimeKind.Utc);
        var change = InfrastructureChangeRequest.Create("Upgrade ingress", "payments-api", "prod", "alice", now);

        change.SubmitForApproval(now.AddMinutes(1));
        change.Approve("bob", now.AddMinutes(2));
        change.ScheduleExecution(now.AddHours(1), now.AddMinutes(3));
        change.MarkExecutionStarted(now.AddHours(1).AddMinutes(1));
        change.MarkExecutionCompleted(now.AddHours(1).AddMinutes(10));

        Assert.That(change.Status, Is.EqualTo(ChangeStatus.Completed));
        Assert.That(change.ApprovedBy, Is.EqualTo("bob"));
        Assert.That(change.ScheduledAtUtc, Is.EqualTo(now.AddHours(1)));
    }

    [Test]
    public void Reject_requires_pending_approval()
    {
        var now = DateTime.UtcNow;
        var change = InfrastructureChangeRequest.Create("Rotate cert", "edge", "staging", "alice", now);

        var exception = Assert.Throws<DomainRuleViolationException>(() => change.Reject("bob", "missing rollout plan", now.AddMinutes(1)));

        Assert.That(exception!.Message, Is.EqualTo("Only pending requests can be rejected."));
    }
}
