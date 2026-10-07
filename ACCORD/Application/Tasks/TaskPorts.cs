using ACCORD.Application.Common;

namespace ACCORD.Application.Tasks;

public interface ITaskReader
{
    Task<OperationResult> ListAsync(string? accessToken, CancellationToken cancellationToken);
}

public interface ITaskWriter
{
    Task<OperationResult> CreateAsync(string title, string? accessToken, CancellationToken cancellationToken);
    Task<OperationResult> SetCompletionAsync(Guid id, bool isComplete, string? accessToken, CancellationToken cancellationToken);
    Task<OperationResult> DeleteAsync(Guid id, string? accessToken, CancellationToken cancellationToken);
}
