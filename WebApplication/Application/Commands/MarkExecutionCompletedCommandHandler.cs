using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionCompletedCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<MarkExecutionCompletedCommand, CommandResult>
{
    public MarkExecutionCompletedCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(MarkExecutionCompletedCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.MarkExecutionCompleted(DateTime.UtcNow),
            "Execution marked as completed.",
            cancellationToken);
    }
}

