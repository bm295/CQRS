using System.Text.Json;

namespace ACCORD.Application.Common;

public sealed record OperationResult(int StatusCode, string? Json = null)
{
    public static OperationResult Error(int statusCode, string message) =>
        new(statusCode, JsonSerializer.Serialize(new { message }));
}

public interface ICommandHandler<in TCommand>
{
    Task<OperationResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery>
{
    Task<OperationResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
