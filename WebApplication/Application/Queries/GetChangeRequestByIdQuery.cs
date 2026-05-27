using MediatR;

namespace WebApplication.Application.Queries;

public sealed record GetChangeRequestByIdQuery(Guid Id) : IRequest<ChangeRequestDetailsVm?>;
