using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Persistence;
namespace WebApplication.Application.AI;
public sealed class ContextBuilder(AdvisorDb db)
{
    // A conservative UTF-8 byte budget bounds even non-English text. No facts are truncated.
    public const int MaxContextBytes = 12000;
    public async Task<AdvisorContext> BuildAsync(Guid id, CancellationToken ct)
    {
        var change = await db.Changes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Change not found.");
        var plan = await db.Plans.AsNoTracking().SingleOrDefaultAsync(x => x.ChangeId == id, ct);
        var policies = await db.Knowledge.AsNoTracking().Where(x => x.SystemName == change.SystemName && x.Environment == change.Environment)
            .OrderBy(x => x.Id).ToArrayAsync(ct);
        var lessons = await (from l in db.Lessons.AsNoTracking() join c in db.Changes on l.ChangeId equals c.Id
            where l.Confirmed && l.ExpiresUtc > DateTime.UtcNow && c.SystemName == change.SystemName && c.Environment == change.Environment
            orderby l.ExpiresUtc descending, l.Id select l).Take(3).ToArrayAsync(ct);
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { change, plan, policies, lessons }))));
        var evidence = new List<Evidence>();
        // Current facts precede mandatory policies; historical lessons are advisory.
        evidence.Add(new($"change:{id}", JsonSerializer.Serialize(change)));
        evidence.AddRange(policies.Select(x => new Evidence($"knowledge:{x.Id}:v{x.Version}", x.Text)));
        var context = new AdvisorContext(id, change.Status.ToString(), plan, evidence.ToArray(), fingerprint);
        if (Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(context)) > MaxContextBytes)
            throw new InvalidOperationException("Applicable policies and change exceed context budget; shorten inputs before analysis.");
        foreach (var lesson in lessons)
        {
            var candidate = context with { Evidence = context.Evidence.Append(new Evidence($"lesson:{lesson.Id}:change:{lesson.ChangeId}", lesson.Text)).ToArray() };
            if (Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(candidate)) <= MaxContextBytes) context = candidate;
        }
        return context;
    }
}
