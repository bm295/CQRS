using System.ComponentModel.DataAnnotations;
using MediatR;

namespace WebApplication.Application.Commands;

public sealed class ScheduleExecutionCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }

    [Required]
    public DateTime ScheduledAtUtc { get; set; }
}
