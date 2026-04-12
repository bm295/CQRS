using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class SubmitForApprovalCommandHandler : ChangeRequestCommandHandlerBase
{
    public SubmitForApprovalCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> HandleAsync(SubmitForApprovalCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.SubmitForApproval(DateTime.UtcNow),
            "Change request submitted for approval.",
            cancellationToken);
    }
}
