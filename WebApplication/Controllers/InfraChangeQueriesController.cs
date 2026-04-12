using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Queries;

namespace WebApplication.Controllers;

[ApiController]
public sealed class InfraChangeQueriesController : ControllerBase
{
    private readonly GetChangeRequestByIdQueryHandler _detailsHandler;
    private readonly ListPendingApprovalsQueryHandler _pendingApprovalsHandler;
    private readonly ListScheduledChangesQueryHandler _scheduledChangesHandler;
    private readonly ListFailedChangesQueryHandler _failedChangesHandler;
    private readonly GetOperationalChangeSummaryQueryHandler _summaryHandler;

    public InfraChangeQueriesController(
        GetChangeRequestByIdQueryHandler detailsHandler,
        ListPendingApprovalsQueryHandler pendingApprovalsHandler,
        ListScheduledChangesQueryHandler scheduledChangesHandler,
        ListFailedChangesQueryHandler failedChangesHandler,
        GetOperationalChangeSummaryQueryHandler summaryHandler)
    {
        _detailsHandler = detailsHandler;
        _pendingApprovalsHandler = pendingApprovalsHandler;
        _scheduledChangesHandler = scheduledChangesHandler;
        _failedChangesHandler = failedChangesHandler;
        _summaryHandler = summaryHandler;
    }

    [HttpGet("infra/changes/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _detailsHandler.HandleAsync(new GetChangeRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("infra/approvals/pending")]
    public async Task<IReadOnlyList<PendingApprovalVm>> PendingApprovals(CancellationToken cancellationToken)
    {
        return await _pendingApprovalsHandler.HandleAsync(new ListPendingApprovalsQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/scheduled")]
    public async Task<IReadOnlyList<ScheduledChangeVm>> Scheduled(CancellationToken cancellationToken)
    {
        return await _scheduledChangesHandler.HandleAsync(new ListScheduledChangesQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/failed")]
    public async Task<IReadOnlyList<FailedChangeVm>> Failed(CancellationToken cancellationToken)
    {
        return await _failedChangesHandler.HandleAsync(new ListFailedChangesQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/summary")]
    public Task<OpsSummaryVm> Summary(CancellationToken cancellationToken)
    {
        return _summaryHandler.HandleAsync(new GetOperationalChangeSummaryQuery(), cancellationToken);
    }
}
