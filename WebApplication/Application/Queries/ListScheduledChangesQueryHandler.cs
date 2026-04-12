using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class ListScheduledChangesQueryHandler
{
    private readonly IChangeReadModelStore _readModelStore;

    public ListScheduledChangesQueryHandler(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task<IReadOnlyList<ScheduledChangeVm>> HandleAsync(ListScheduledChangesQuery query, CancellationToken cancellationToken = default)
    {
        return _readModelStore.ListScheduledChangesAsync(cancellationToken);
    }
}
