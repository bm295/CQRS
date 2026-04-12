using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class ScheduleExecutionCommandHandler : ChangeRequestCommandHandlerBase
{
    public ScheduleExecutionCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> HandleAsync(ScheduleExecutionCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.ScheduleExecution(command.ScheduledAtUtc, DateTime.UtcNow),
            "Execution scheduled.",
            cancellationToken);
    }
}
