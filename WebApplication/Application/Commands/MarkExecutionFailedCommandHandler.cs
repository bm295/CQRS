using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionFailedCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<MarkExecutionFailedCommand, CommandResult>
{
    public MarkExecutionFailedCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(MarkExecutionFailedCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.MarkExecutionFailed(command.FailureReason, DateTime.UtcNow),
            "Execution marked as failed.",
            cancellationToken);
    }

    public Task<CommandResult> HandleAsync(MarkExecutionFailedCommand command, CancellationToken cancellationToken = default)
        => Handle(command, cancellationToken);
}

