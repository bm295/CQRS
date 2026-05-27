using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Queries;

namespace WebApplication.Controllers;

[ApiController]
public sealed class InfraChangeQueriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InfraChangeQueriesController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("infra/changes/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChangeRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("infra/approvals/pending")]
    public async Task<IReadOnlyList<PendingApprovalVm>> PendingApprovals(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListPendingApprovalsQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/scheduled")]
    public async Task<IReadOnlyList<ScheduledChangeVm>> Scheduled(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListScheduledChangesQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/failed")]
    public async Task<IReadOnlyList<FailedChangeVm>> Failed(CancellationToken cancellationToken)
    {
        return await _mediator.Send(new ListFailedChangesQuery(), cancellationToken);
    }

    [HttpGet("infra/changes/summary")]
    public Task<OpsSummaryVm> Summary(CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetOperationalChangeSummaryQuery(), cancellationToken);
    }
}

