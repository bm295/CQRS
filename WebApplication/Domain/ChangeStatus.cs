namespace WebApplication.Domain;

public enum ChangeStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Scheduled = 3,
    InProgress = 4,
    Completed = 5,
    Failed = 6,
    Rejected = 7
}
