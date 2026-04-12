using Microsoft.AspNetCore.Mvc;
using WebApplication.Application.Commands;
using WebApplication.Domain;

namespace WebApplication.Controllers;

[Route("infra/changes")]
public sealed class InfraChangeCommandsController : Controller
{
    private readonly CreateChangeRequestCommandHandler _createHandler;
    private readonly SubmitForApprovalCommandHandler _submitHandler;
    private readonly ApproveChangeRequestCommandHandler _approveHandler;
    private readonly RejectChangeRequestCommandHandler _rejectHandler;
    private readonly ScheduleExecutionCommandHandler _scheduleHandler;
    private readonly MarkExecutionStartedCommandHandler _startHandler;
    private readonly MarkExecutionCompletedCommandHandler _completeHandler;
    private readonly MarkExecutionFailedCommandHandler _failHandler;

    public InfraChangeCommandsController(
        CreateChangeRequestCommandHandler createHandler,
        SubmitForApprovalCommandHandler submitHandler,
        ApproveChangeRequestCommandHandler approveHandler,
        RejectChangeRequestCommandHandler rejectHandler,
        ScheduleExecutionCommandHandler scheduleHandler,
        MarkExecutionStartedCommandHandler startHandler,
        MarkExecutionCompletedCommandHandler completeHandler,
        MarkExecutionFailedCommandHandler failHandler)
    {
        _createHandler = createHandler;
        _submitHandler = submitHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _scheduleHandler = scheduleHandler;
        _startHandler = startHandler;
        _completeHandler = completeHandler;
        _failHandler = failHandler;
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateChangeRequestCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/InfraPages/Create.cshtml", command);
        }

        try
        {
            var result = await _createHandler.HandleAsync(command, cancellationToken);
            TempData["StatusMessage"] = result.Message;
            return RedirectToAction("Details", "InfraPages", new { id = result.Id });
        }
        catch (DomainRuleViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/InfraPages/Create.cshtml", command);
        }
    }

    [HttpPost("{id:guid}/submit")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteTransitionAsync(
            () => _submitHandler.HandleAsync(new SubmitForApprovalCommand(id), cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/approve")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(Guid id, ApproveChangeRequestCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return ExecuteTransitionAsync(
            () => _approveHandler.HandleAsync(command, cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/reject")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(Guid id, RejectChangeRequestCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return ExecuteTransitionAsync(
            () => _rejectHandler.HandleAsync(command, cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/schedule")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Schedule(Guid id, ScheduleExecutionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return ExecuteTransitionAsync(
            () => _scheduleHandler.HandleAsync(command, cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/start")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteTransitionAsync(
            () => _startHandler.HandleAsync(new MarkExecutionStartedCommand(id), cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/complete")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteTransitionAsync(
            () => _completeHandler.HandleAsync(new MarkExecutionCompletedCommand(id), cancellationToken),
            id);
    }

    [HttpPost("{id:guid}/fail")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Fail(Guid id, MarkExecutionFailedCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return ExecuteTransitionAsync(
            () => _failHandler.HandleAsync(command, cancellationToken),
            id);
    }

    private async Task<IActionResult> ExecuteTransitionAsync(Func<Task<CommandResult>> action, Guid id)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "The command payload is invalid.";
            return RedirectToAction("Details", "InfraPages", new { id });
        }

        try
        {
            var result = await action();
            TempData["StatusMessage"] = result.Message;
        }
        catch (DomainRuleViolationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Dashboard", "InfraPages");
        }

        return RedirectToAction("Details", "InfraPages", new { id });
    }
}
