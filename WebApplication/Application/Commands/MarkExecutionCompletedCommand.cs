using MediatR;

namespace WebApplication.Application.Commands;

public sealed record MarkExecutionCompletedCommand(Guid Id) : IRequest<CommandResult>;
