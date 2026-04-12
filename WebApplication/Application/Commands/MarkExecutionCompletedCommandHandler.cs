using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionCompletedCommandHandler : ChangeRequestCommandHandlerBase
{
    public MarkExecutionCompletedCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> HandleAsync(MarkExecutionCompletedCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.MarkExecutionCompleted(DateTime.UtcNow),
            "Execution marked as completed.",
            cancellationToken);
    }
}
