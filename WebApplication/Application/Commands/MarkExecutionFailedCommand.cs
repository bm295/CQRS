using System.ComponentModel.DataAnnotations;

namespace WebApplication.Application.Commands;

public sealed class MarkExecutionFailedCommand
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(400)]
    public string FailureReason { get; set; } = string.Empty;
}
