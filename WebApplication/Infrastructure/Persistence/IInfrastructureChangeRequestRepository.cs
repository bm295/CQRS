using WebApplication.Domain;

namespace WebApplication.Infrastructure.Persistence;

public interface IInfrastructureChangeRequestRepository
{
    Task AddAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default);
    Task<InfrastructureChangeRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(InfrastructureChangeRequest changeRequest, CancellationToken cancellationToken = default);
}
