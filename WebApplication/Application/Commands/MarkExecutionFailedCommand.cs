using System.ComponentModel.DataAnnotations;
using MediatR;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionFailedCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(400)]
    public string FailureReason { get; set; } = string.Empty;
}
