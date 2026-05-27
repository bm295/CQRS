using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class ScheduleExecutionCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<ScheduleExecutionCommand, CommandResult>
{
    public ScheduleExecutionCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(ScheduleExecutionCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.ScheduleExecution(command.ScheduledAtUtc, DateTime.UtcNow),
            "Execution scheduled.",
            cancellationToken);
    }

    public Task<CommandResult> HandleAsync(ScheduleExecutionCommand command, CancellationToken cancellationToken = default)
        => Handle(command, cancellationToken);
}

