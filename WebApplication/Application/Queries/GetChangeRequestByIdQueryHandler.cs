using MediatR;
using WebApplication.Infrastructure.ReadModels;

namespace WebApplication.Application.Queries;

public sealed class GetChangeRequestByIdQueryHandler : IRequestHandler<GetChangeRequestByIdQuery, ChangeRequestDetailsVm?>
{
    private readonly IChangeReadModelStore _readModelStore;

    public GetChangeRequestByIdQueryHandler(IChangeReadModelStore readModelStore)
    {
        _readModelStore = readModelStore;
    }

    public Task<ChangeRequestDetailsVm?> HandleAsync(GetChangeRequestByIdQuery query, CancellationToken cancellationToken = default)
    {
        return _readModelStore.GetByIdAsync(query.Id, cancellationToken);
    }
    public Task<ChangeRequestDetailsVm?> Handle(GetChangeRequestByIdQuery query, CancellationToken cancellationToken) => HandleAsync(query, cancellationToken);
}
