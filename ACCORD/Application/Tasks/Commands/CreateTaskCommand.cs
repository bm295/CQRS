using ACCORD.Application.Common;

namespace ACCORD.Application.Tasks.Commands;

public sealed record CreateTaskCommand(string? Title, string? AccessToken);

public sealed class CreateTaskCommandHandler(ITaskWriter writer) : ICommandHandler<CreateTaskCommand>
{
    public Task<OperationResult> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title) || command.Title.Trim().Length > 200)
            return Task.FromResult(OperationResult.Error(400, "Tên công việc phải có từ 1 đến 200 ký tự."));
        return writer.CreateAsync(command.Title.Trim(), command.AccessToken, cancellationToken);
    }
}
