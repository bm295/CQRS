namespace WebApplication.Application.AI;
public sealed class Knowledge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SystemName { get; set; } = "";
    public string Environment { get; set; } = "";
    public string Text { get; set; } = "";
    public int Version { get; set; } = 1;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
public sealed class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChangeId { get; set; }
    public string Text { get; set; } = "";
    public bool Confirmed { get; set; }
    public DateTime ExpiresUtc { get; set; } = DateTime.UtcNow.AddDays(90);
}
public sealed class ChangePlan
{
    public Guid ChangeId { get; set; }
    public string Description { get; set; } = "";
    public string Implementation { get; set; } = "";
    public string Rollback { get; set; } = "";
    public string Verification { get; set; } = "";
}
public sealed class Analysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChangeId { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string Fingerprint { get; set; } = "";
    public string ContextJson { get; set; } = "";
    public string ResultJson { get; set; } = "";
    public string Mode { get; set; } = "rules";
}
public sealed record Evidence(string Source, string Text);
public sealed record AdvisorContext(Guid ChangeId, string Status, ChangePlan? Plan, Evidence[] Evidence, string Fingerprint);
public sealed record AdvisorResult(string Readiness, string[] Findings, string[] MissingInformation, string[] Checklist, string[] EvidenceSources);
public interface IChangeAdvisor
{
    Task<AdvisorResult> AnalyzeAsync(AdvisorContext context, CancellationToken ct);
    string Mode { get; }
}
