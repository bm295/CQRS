using ACCORD.Application.Common;

namespace ACCORD.Application.Tasks.Queries;

public sealed record ListTasksQuery(string? AccessToken);

public sealed class ListTasksQueryHandler(ITaskReader reader) : IQueryHandler<ListTasksQuery>
{
    public Task<OperationResult> HandleAsync(ListTasksQuery query, CancellationToken cancellationToken) =>
        reader.ListAsync(query.AccessToken, cancellationToken);
}
