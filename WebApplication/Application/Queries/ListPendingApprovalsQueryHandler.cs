using MediatR;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class ListPendingApprovalsQueryHandler : IRequestHandler<ListPendingApprovalsQuery, IReadOnlyList<PendingApprovalVm>>
{
    private readonly IChangeReadModelStore _readModelStore;

    public ListPendingApprovalsQueryHandler(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task<IReadOnlyList<PendingApprovalVm>> HandleAsync(ListPendingApprovalsQuery query, CancellationToken cancellationToken = default)
    {
        return _readModelStore.ListPendingApprovalsAsync(cancellationToken);
    }
    public Task<IReadOnlyList<PendingApprovalVm>> Handle(ListPendingApprovalsQuery query, CancellationToken cancellationToken) => HandleAsync(query, cancellationToken);
}
