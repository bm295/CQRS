using System.ComponentModel.DataAnnotations;
using MediatR;

namespace WebApplication.Application.Commands;

public sealed class ApproveChangeRequestCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(120)]
    public string ApprovedBy { get; set; } = string.Empty;
}
