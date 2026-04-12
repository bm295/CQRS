using WebApplication.Domain;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public abstract class ChangeRequestCommandHandlerBase
{
    private readonly IInfrastructureChangeRequestRepository _repository;
    private readonly ProjectionUpdater _projectionUpdater;

    protected ChangeRequestCommandHandlerBase(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
    {
        _repository = repository;
        _projectionUpdater = projectionUpdater;
    }

    protected async Task<CommandResult> UpdateAsync(
        Guid id,
        Action<InfrastructureChangeRequest> update,
        string successMessage,
        CancellationToken cancellationToken)
    {
        var changeRequest = await _repository.GetAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Change request '{id}' was not found.");

        update(changeRequest);
        await _repository.UpdateAsync(changeRequest, cancellationToken);
        await _projectionUpdater.UpdateAsync(changeRequest, cancellationToken);
        return new CommandResult(changeRequest.Id, successMessage);
    }
}
