using ACCORD.Application.Common;

namespace ACCORD.Application.Tasks.Commands;

public sealed record DeleteTaskCommand(Guid Id, string? AccessToken);

public sealed class DeleteTaskCommandHandler(ITaskWriter writer) : ICommandHandler<DeleteTaskCommand>
{
    public Task<OperationResult> HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken) =>
        writer.DeleteAsync(command.Id, command.AccessToken, cancellationToken);
}
