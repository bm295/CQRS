using MediatR;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class ListScheduledChangesQueryHandler : IRequestHandler<ListScheduledChangesQuery, IReadOnlyList<ScheduledChangeVm>>
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
    public Task<IReadOnlyList<ScheduledChangeVm>> Handle(ListScheduledChangesQuery query, CancellationToken cancellationToken) => HandleAsync(query, cancellationToken);
}
