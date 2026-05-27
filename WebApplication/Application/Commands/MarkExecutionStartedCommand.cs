using MediatR;

namespace WebApplication.Application.Commands;

public sealed record MarkExecutionStartedCommand(Guid Id) : IRequest<CommandResult>;
