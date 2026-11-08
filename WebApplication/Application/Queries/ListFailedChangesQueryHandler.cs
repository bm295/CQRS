using MediatR;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class ListFailedChangesQueryHandler : IRequestHandler<ListFailedChangesQuery, IReadOnlyList<FailedChangeVm>>
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
    public Task<IReadOnlyList<FailedChangeVm>> Handle(ListFailedChangesQuery query, CancellationToken cancellationToken) => HandleAsync(query, cancellationToken);
}
