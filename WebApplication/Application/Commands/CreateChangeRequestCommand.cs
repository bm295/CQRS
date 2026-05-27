using System.ComponentModel.DataAnnotations;
using MediatR;

namespace WebApplication.Application.Commands;

public sealed class CreateChangeRequestCommand : IRequest<CommandResult>
{
    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string SystemName { get; set; } = string.Empty;

    [Required]
    public string Environment { get; set; } = "dev";

    [Required]
    [StringLength(120)]
    public string RequestedBy { get; set; } = string.Empty;
}
