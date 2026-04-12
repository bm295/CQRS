using WebApplication.Domain;

namespace WebApplication.Infrastructure.ReadModels;

public sealed class ProjectionUpdater
{
    private readonly IChangeReadModelStore _readModelStore;

    public ProjectionUpdater(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task UpdateAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default)
    {
        return _readModelStore.UpsertAsync(changeRequest, cancellationToken);
    }
}
