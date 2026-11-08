using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WebApplication.Application.AI;
using WebApplication.Domain;
using WebApplication.Infrastructure.AI;
using WebApplication.Infrastructure.Persistence;
namespace WebApplication.Tests.Application;
[TestFixture]
public sealed class ChangeAdvisorTests
{
    private string _path = null!;
    private AdvisorDb Open() => new(new DbContextOptionsBuilder<AdvisorDb>().UseSqlite($"Data Source={_path}").Options);
    [SetUp] public void Setup() { _path = Path.Combine(Path.GetTempPath(), $"advisor-{Guid.NewGuid()}.db"); using var db = Open(); db.Database.EnsureCreated(); }
    [TearDown] public void Cleanup() { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); File.Delete(_path); }
    private static InfrastructureChangeRequest Change() => InfrastructureChangeRequest.Create("Rotate certificate", "edge", "prod", "alice", DateTime.UtcNow);
    [Test] public async Task Change_and_analysis_survive_a_new_context_and_become_stale_after_transition()
    {
        Guid id;
        using (var db = Open())
        {
            var change = Change(); id = change.Id; await new SqliteChangeRepository(db).AddAsync(change);
            await new AnalyzeChangeHandler(db, new ContextBuilder(db), new RuleChangeAdvisor()).Handle(new(id), default);
        }
        using (var db = Open())
        {
            var handler = new LatestAnalysisHandler(db, new ContextBuilder(db));
            Assert.That((await handler.Handle(new(id), default))!.IsStale, Is.False);
            var change = (await db.Changes.FindAsync(id))!; change.SubmitForApproval(DateTime.UtcNow);
            await db.SaveChangesAsync();
            Assert.That((await handler.Handle(new(id), default))!.IsStale, Is.True);
        }
    }
    [Test] public async Task Context_excludes_other_scopes_unconfirmed_and_expired_lessons()
    {
        using var db = Open(); var change = Change(); db.Changes.Add(change);
        var included = new Lesson { ChangeId = change.Id, Text = "Check certificate chain", Confirmed = true };
        db.Lessons.AddRange(included, new Lesson { ChangeId = change.Id, Text = "Unconfirmed" }, new Lesson { ChangeId = change.Id, Text = "Expired", Confirmed = true, ExpiresUtc = DateTime.UtcNow.AddMinutes(-1) });
        db.Knowledge.AddRange(new Knowledge { SystemName = "edge", Environment = "prod", Text = "Rollback required" }, new Knowledge { SystemName = "other", Environment = "prod", Text = "Unrelated" });
        await db.SaveChangesAsync();
        var c = await new ContextBuilder(db).BuildAsync(change.Id, default);
        Assert.That(c.Evidence.Count(x => x.Source.StartsWith("lesson:")), Is.EqualTo(1));
        Assert.That(c.Evidence.Any(x => x.Text == "Unrelated"), Is.False);
        var result = await new RuleChangeAdvisor().AnalyzeAsync(c, default);
        Assert.That(result.MissingInformation, Does.Contain("Rollback plan"));
        Assert.That(result.Readiness, Is.EqualTo("Needs human review"));
    }
    [Test] public async Task Policy_update_invalidates_prior_analysis()
    {
        using var db = Open(); var change = Change(); db.Changes.Add(change);
        var policy = new Knowledge { SystemName = "edge", Environment = "prod", Text = "Old rule" }; db.Knowledge.Add(policy); await db.SaveChangesAsync();
        await new AnalyzeChangeHandler(db, new ContextBuilder(db), new RuleChangeAdvisor()).Handle(new(change.Id), default);
        policy.Text = "New rule"; policy.Version++; await db.SaveChangesAsync();
        Assert.That((await new LatestAnalysisHandler(db, new ContextBuilder(db)).Handle(new(change.Id), default))!.IsStale, Is.True);
    }
    [Test] public async Task Mandatory_context_over_budget_fails_without_silently_omitting_policies()
    {
        using var db = Open(); var change = Change(); db.Changes.Add(change);
        db.Knowledge.Add(new Knowledge { SystemName = "edge", Environment = "prod", Text = new string('x', 13000) }); await db.SaveChangesAsync();
        Assert.ThrowsAsync<InvalidOperationException>(() => new ContextBuilder(db).BuildAsync(change.Id, default));
    }
}
