using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class RejectChangeRequestCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<RejectChangeRequestCommand, CommandResult>
{
    public RejectChangeRequestCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(RejectChangeRequestCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.Reject(command.ApprovedBy, command.Reason, DateTime.UtcNow),
            "Change request rejected.",
            cancellationToken);
    }
}

