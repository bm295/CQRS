using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class RejectChangeRequestCommandHandler : ChangeRequestCommandHandlerBase
{
    public RejectChangeRequestCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> HandleAsync(RejectChangeRequestCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.Reject(command.ApprovedBy, command.Reason, DateTime.UtcNow),
            "Change request rejected.",
            cancellationToken);
    }
}
