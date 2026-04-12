using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class GetOperationalChangeSummaryQueryHandler
{
    private readonly IChangeReadModelStore _readModelStore;

    public GetOperationalChangeSummaryQueryHandler(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task<OpsSummaryVm> HandleAsync(GetOperationalChangeSummaryQuery query, CancellationToken cancellationToken = default)
    {
        return _readModelStore.GetSummaryAsync(cancellationToken);
    }
}
