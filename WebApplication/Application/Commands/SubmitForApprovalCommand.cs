using MediatR;

namespace WebApplication.Application.Commands;

public sealed record SubmitForApprovalCommand(Guid Id) : IRequest<CommandResult>;
