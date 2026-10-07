using ACCORD.Application.Common;

namespace ACCORD.Application.Tasks.Commands;

public sealed record SetTaskCompletionCommand(Guid Id, bool IsComplete, string? AccessToken);

public sealed class SetTaskCompletionCommandHandler(ITaskWriter writer) : ICommandHandler<SetTaskCompletionCommand>
{
    public Task<OperationResult> HandleAsync(SetTaskCompletionCommand command, CancellationToken cancellationToken) =>
        writer.SetCompletionAsync(command.Id, command.IsComplete, command.AccessToken, cancellationToken);
}
