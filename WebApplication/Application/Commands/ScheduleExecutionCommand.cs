using System.ComponentModel.DataAnnotations;

namespace WebApplication.Application.Commands;

public sealed class ScheduleExecutionCommand
{
    public Guid Id { get; set; }

    [Required]
    public DateTime ScheduledAtUtc { get; set; }
}
