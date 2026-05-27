using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionStartedCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<MarkExecutionStartedCommand, CommandResult>
{
    public MarkExecutionStartedCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(MarkExecutionStartedCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.MarkExecutionStarted(DateTime.UtcNow),
            "Execution marked as started.",
            cancellationToken);
    }

    public Task<CommandResult> HandleAsync(MarkExecutionStartedCommand command, CancellationToken cancellationToken = default)
        => Handle(command, cancellationToken);
}

