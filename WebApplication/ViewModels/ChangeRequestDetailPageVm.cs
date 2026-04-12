using WebApplication.Application.Commands;
using WebApplication.Application.Queries;

namespace WebApplication.ViewModels;

public sealed class ChangeRequestDetailPageVm
{
    public required ChangeRequestDetailsVm Details { get; init; }
    public required ApproveChangeRequestCommand ApproveCommand { get; init; }
    public required RejectChangeRequestCommand RejectCommand { get; init; }
    public required ScheduleExecutionCommand ScheduleCommand { get; init; }
    public required MarkExecutionFailedCommand FailCommand { get; init; }
}
