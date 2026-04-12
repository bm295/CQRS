using NUnit.Framework;
using WebApplication.Application.Commands;
using WebApplication.Application.Queries;
using WebApplication.Domain;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Tests.Application;

[TestFixture]
public sealed class CommandAndQueryFlowTests
{
    [Test]
    public async Task Commands_update_projection_and_queries_return_read_models()
    {
        var repository = new InMemoryInfrastructureChangeRequestRepository();
        var readStore = new InMemoryChangeReadModelStore();
        var projectionUpdater = new ProjectionUpdater(readStore);

        var create = new CreateChangeRequestCommandHandler(repository, projectionUpdater);
        var submit = new SubmitForApprovalCommandHandler(repository, projectionUpdater);
        var approve = new ApproveChangeRequestCommandHandler(repository, projectionUpdater);
        var schedule = new ScheduleExecutionCommandHandler(repository, projectionUpdater);
        var start = new MarkExecutionStartedCommandHandler(repository, projectionUpdater);
        var fail = new MarkExecutionFailedCommandHandler(repository, projectionUpdater);

        var result = await create.HandleAsync(new CreateChangeRequestCommand
        {
            Title = "Patch Kubernetes nodes",
            SystemName = "cluster-west",
            Environment = "prod",
            RequestedBy = "sre@ops"
        });

        await submit.HandleAsync(new SubmitForApprovalCommand(result.Id));
        await approve.HandleAsync(new ApproveChangeRequestCommand { Id = result.Id, ApprovedBy = "lead@ops" });
        await schedule.HandleAsync(new ScheduleExecutionCommand { Id = result.Id, ScheduledAtUtc = DateTime.UtcNow.AddHours(2) });
        await start.HandleAsync(new MarkExecutionStartedCommand(result.Id));
        await fail.HandleAsync(new MarkExecutionFailedCommand { Id = result.Id, FailureReason = "Health checks failed" });

        var details = await new GetChangeRequestByIdQueryHandler(readStore).HandleAsync(new GetChangeRequestByIdQuery(result.Id));
        var failed = await new ListFailedChangesQueryHandler(readStore).HandleAsync(new ListFailedChangesQuery());
        var summary = await new GetOperationalChangeSummaryQueryHandler(readStore).HandleAsync(new GetOperationalChangeSummaryQuery());

        Assert.Multiple(() =>
        {
            Assert.That(details, Is.Not.Null);
            Assert.That(details!.Status, Is.EqualTo(ChangeStatus.Failed));
            Assert.That(failed.Select(x => x.Id), Contains.Item(result.Id));
            Assert.That(summary.FailedChanges, Is.EqualTo(1));
        });
    }
}
