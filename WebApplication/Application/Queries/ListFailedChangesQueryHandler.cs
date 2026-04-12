using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class ListFailedChangesQueryHandler
{
    private readonly IChangeReadModelStore _readModelStore;

    public ListFailedChangesQueryHandler(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task<IReadOnlyList<FailedChangeVm>> HandleAsync(ListFailedChangesQuery query, CancellationToken cancellationToken = default)
    {
        return _readModelStore.ListFailedChangesAsync(cancellationToken);
    }
}
