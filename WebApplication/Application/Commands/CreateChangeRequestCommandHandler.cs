using WebApplication.Domain;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Commands;

public sealed class CreateChangeRequestCommandHandler
{
    private readonly IInfrastructureChangeRequestRepository _repository;
    private readonly ProjectionUpdater _projectionUpdater;

    public CreateChangeRequestCommandHandler(
        IInfrastructureChangeRequestRepository repository,
        ProjectionUpdater projectionUpdater)
    {
        _repository = repository;
        _projectionUpdater = projectionUpdater;
    }

    public async Task<CommandResult> HandleAsync(CreateChangeRequestCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var changeRequest = InfrastructureChangeRequest.Create(command.Title, command.SystemName, command.Environment, command.RequestedBy, now);
        await _repository.AddAsync(changeRequest, cancellationToken);
        await _projectionUpdater.UpdateAsync(changeRequest, cancellationToken);
        return new CommandResult(changeRequest.Id, "Change request created.");
    }
}
