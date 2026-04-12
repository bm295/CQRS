using System.ComponentModel.DataAnnotations;

namespace WebApplication.Application.Commands;

public sealed class ApproveChangeRequestCommand
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(120)]
    public string ApprovedBy { get; set; } = string.Empty;
}
