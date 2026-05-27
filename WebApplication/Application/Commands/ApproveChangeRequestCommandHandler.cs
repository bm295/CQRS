using MediatR;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class ApproveChangeRequestCommandHandler : ChangeRequestCommandHandlerBase, IRequestHandler<ApproveChangeRequestCommand, CommandResult>
{
    public ApproveChangeRequestCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
        : base(repository, projectionUpdater)
    {
    }

    public Task<CommandResult> Handle(ApproveChangeRequestCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            command.Id,
            changeRequest => changeRequest.Approve(command.ApprovedBy, DateTime.UtcNow),
            "Change request approved.",
            cancellationToken);
    }

    public Task<CommandResult> HandleAsync(ApproveChangeRequestCommand command, CancellationToken cancellationToken = default)
        => Handle(command, cancellationToken);
}

