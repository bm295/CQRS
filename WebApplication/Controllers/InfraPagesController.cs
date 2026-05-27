using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Commands;
using WebApplication.Application.Queries;
using WebApplication.ViewModels;

namespace WebApplication.Controllers;

public sealed class InfraPagesController : Controller
{
    private readonly IMediator _mediator;

    public InfraPagesController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("/infra/changes/new")]
    public IActionResult Create()
    {
        return View(new CreateChangeRequestCommand());
    }

    [HttpGet("/infra/changes/{id:guid}/view")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var details = await _mediator.Send(new GetChangeRequestByIdQuery(id), cancellationToken);
        if (details is null)
        {
            return NotFound();
        }

        var viewModel = new ChangeRequestDetailPageVm
        {
            Details = details,
            ApproveCommand = new ApproveChangeRequestCommand { Id = id },
            RejectCommand = new RejectChangeRequestCommand { Id = id },
            ScheduleCommand = new ScheduleExecutionCommand { Id = id, ScheduledAtUtc = DateTime.UtcNow.AddHours(2) },
            FailCommand = new MarkExecutionFailedCommand { Id = id }
        };

        return View(viewModel);
    }

    [HttpGet("/infra/approvals")]
    public async Task<IActionResult> Approvals(CancellationToken cancellationToken)
    {
        var results = await _mediator.Send(new ListPendingApprovalsQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/schedule")]
    public async Task<IActionResult> Schedule(CancellationToken cancellationToken)
    {
        var results = await _mediator.Send(new ListScheduledChangesQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/failures")]
    public async Task<IActionResult> Failures(CancellationToken cancellationToken)
    {
        var results = await _mediator.Send(new ListFailedChangesQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/dashboard")]
    [HttpGet("/")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var viewModel = new DashboardPageVm
        {
            Summary = await _mediator.Send(new GetOperationalChangeSummaryQuery(), cancellationToken),
            PendingApprovals = await _mediator.Send(new ListPendingApprovalsQuery(), cancellationToken),
            ScheduledChanges = await _mediator.Send(new ListScheduledChangesQuery(), cancellationToken),
            FailedChanges = await _mediator.Send(new ListFailedChangesQuery(), cancellationToken)
        };

        return View(viewModel);
    }
}

