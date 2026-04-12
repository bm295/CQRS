using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Commands;
using WebApplication.Application.Queries;
using WebApplication.ViewModels;

namespace WebApplication.Controllers;

public sealed class InfraPagesController : Controller
{
    private readonly GetChangeRequestByIdQueryHandler _detailsHandler;
    private readonly ListPendingApprovalsQueryHandler _pendingApprovalsHandler;
    private readonly ListScheduledChangesQueryHandler _scheduledChangesHandler;
    private readonly ListFailedChangesQueryHandler _failedChangesHandler;
    private readonly GetOperationalChangeSummaryQueryHandler _summaryHandler;

    public InfraPagesController(
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

    [HttpGet("/infra/changes/new")]
    public IActionResult Create()
    {
        return View(new CreateChangeRequestCommand());
    }

    [HttpGet("/infra/changes/{id:guid}/view")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var details = await _detailsHandler.HandleAsync(new GetChangeRequestByIdQuery(id), cancellationToken);
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
        var results = await _pendingApprovalsHandler.HandleAsync(new ListPendingApprovalsQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/schedule")]
    public async Task<IActionResult> Schedule(CancellationToken cancellationToken)
    {
        var results = await _scheduledChangesHandler.HandleAsync(new ListScheduledChangesQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/failures")]
    public async Task<IActionResult> Failures(CancellationToken cancellationToken)
    {
        var results = await _failedChangesHandler.HandleAsync(new ListFailedChangesQuery(), cancellationToken);
        return View(results);
    }

    [HttpGet("/infra/dashboard")]
    [HttpGet("/")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var viewModel = new DashboardPageVm
        {
            Summary = await _summaryHandler.HandleAsync(new GetOperationalChangeSummaryQuery(), cancellationToken),
            PendingApprovals = await _pendingApprovalsHandler.HandleAsync(new ListPendingApprovalsQuery(), cancellationToken),
            ScheduledChanges = await _scheduledChangesHandler.HandleAsync(new ListScheduledChangesQuery(), cancellationToken),
            FailedChanges = await _failedChangesHandler.HandleAsync(new ListFailedChangesQuery(), cancellationToken)
        };

        return View(viewModel);
    }
}
