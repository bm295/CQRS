using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Persistence;
namespace WebApplication.Application.AI;
public sealed record AnalyzeChange(Guid Id) : IRequest<Analysis>;
public sealed record LatestAnalysis(Guid Id) : IRequest<AnalysisView?>;
public sealed record AnalysisView(Analysis Analysis, bool IsStale);
public sealed class AnalyzeChangeHandler(AdvisorDb db, ContextBuilder builder, IChangeAdvisor advisor) : IRequestHandler<AnalyzeChange, Analysis>
{
    public async Task<Analysis> Handle(AnalyzeChange request, CancellationToken ct)
    {
        var context = await builder.BuildAsync(request.Id, ct);
        var result = await advisor.AnalyzeAsync(context, ct);
        // Preserve the exact snapshot, even if a concurrent edit makes it stale while the model runs.
        var analysis = new Analysis { ChangeId = request.Id, Fingerprint = context.Fingerprint,
            ContextJson = JsonSerializer.Serialize(context), ResultJson = JsonSerializer.Serialize(result), Mode = advisor.Mode };
        db.Analyses.Add(analysis); await db.SaveChangesAsync(ct); return analysis;
    }
}
public sealed class LatestAnalysisHandler(AdvisorDb db, ContextBuilder builder) : IRequestHandler<LatestAnalysis, AnalysisView?>
{
    public async Task<AnalysisView?> Handle(LatestAnalysis request, CancellationToken ct)
    {
        var context = await builder.BuildAsync(request.Id, ct);
        var latest = await db.Analyses.AsNoTracking().Where(x => x.ChangeId == request.Id).OrderByDescending(x => x.CreatedUtc).FirstOrDefaultAsync(ct);
        return latest is null ? null : new AnalysisView(latest, latest.Fingerprint != context.Fingerprint || latest.CreatedUtc < DateTime.UtcNow.AddHours(-1));
    }
}
