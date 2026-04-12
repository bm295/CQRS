using System.Collections.Concurrent;
using WebApplication.Domain;

namespace WebApplication.Infrastructure.Persistence;

public sealed class InMemoryInfrastructureChangeRequestRepository : IInfrastructureChangeRequestRepository
{
    private readonly ConcurrentDictionary<Guid, InfrastructureChangeRequest> _store = new();

    public Task AddAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default)
    {
        if (!_store.TryAdd(changeRequest.Id, changeRequest))
        {
            throw new InvalidOperationException($"Change request '{changeRequest.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<InfrastructureChangeRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var changeRequest);
        return Task.FromResult(changeRequest);
    }

    public Task UpdateAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default)
    {
        _store[changeRequest.Id] = changeRequest;
        return Task.CompletedTask;
    }
}
