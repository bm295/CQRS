using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Application.AI;
using WebApplication.Domain;
using WebApplication.Infrastructure.Persistence;
namespace WebApplication.Controllers;
// Same single-workspace trust boundary as the existing demo. Do not expose publicly without authentication.
[AutoValidateAntiforgeryToken]
public sealed class ChangeAdvisorController(AdvisorDb db, IMediator mediator) : Controller
{
    [HttpGet("/infra/changes/{id:guid}/advisor")]
    public async Task<IActionResult> Index(Guid id, CancellationToken ct)
    {
        var change = await db.Changes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (change is null) return NotFound();
        ViewBag.Change = change;
        ViewBag.Plan = await db.Plans.AsNoTracking().SingleOrDefaultAsync(x => x.ChangeId == id, ct) ?? new ChangePlan { ChangeId = id };
        ViewBag.Knowledge = await db.Knowledge.AsNoTracking().Where(x => x.SystemName == change.SystemName && x.Environment == change.Environment).ToListAsync(ct);
        ViewBag.Lessons = await db.Lessons.AsNoTracking().Where(x => x.ChangeId == id).ToListAsync(ct);
        try { ViewBag.Latest = await mediator.Send(new LatestAnalysis(id), ct); }
        catch (InvalidOperationException e) { ViewBag.Error = e.Message; }
        return View();
    }
    [HttpPost("/infra/changes/{id:guid}/plan")]
    public async Task<IActionResult> Plan(Guid id, string description, string implementation, string rollback, string verification, CancellationToken ct)
    {
        if (!await db.Changes.AnyAsync(x => x.Id == id, ct)) return NotFound();
        if (!Valid(description, implementation, rollback, verification)) return BadRequest("Fields must contain 1–1500 characters.");
        var plan = await db.Plans.SingleOrDefaultAsync(x => x.ChangeId == id, ct);
        if (plan is null) { plan = new ChangePlan { ChangeId = id }; db.Plans.Add(plan); }
        plan.Description = description; plan.Implementation = implementation; plan.Rollback = rollback; plan.Verification = verification;
        await db.SaveChangesAsync(ct); return Back(id);
    }
    [HttpPost("/infra/changes/{id:guid}/knowledge")]
    public async Task<IActionResult> Knowledge(Guid id, string text, Guid? knowledgeId, CancellationToken ct)
    {
        var change = await db.Changes.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (change is null) return NotFound();
        if (!Valid(text)) return BadRequest("Policy must contain 1–1500 characters.");
        var policy = knowledgeId.HasValue ? await db.Knowledge.SingleOrDefaultAsync(x => x.Id == knowledgeId && x.SystemName == change.SystemName && x.Environment == change.Environment, ct) : null;
        if (knowledgeId.HasValue && policy is null) return NotFound();
        if (policy is null) { policy = new Knowledge { SystemName = change.SystemName, Environment = change.Environment }; db.Knowledge.Add(policy); }
        else policy.Version++;
        policy.Text = text; policy.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Back(id);
    }
    [HttpPost("/infra/changes/{id:guid}/lessons")]
    public async Task<IActionResult> Lesson(Guid id, string text, CancellationToken ct)
    {
        var change = await db.Changes.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (change is null) return NotFound();
        if (change.Status is not (ChangeStatus.Completed or ChangeStatus.Failed)) return BadRequest("Lessons require a completed or failed change.");
        if (!Valid(text)) return BadRequest("Lesson must contain 1–1500 characters.");
        db.Lessons.Add(new Lesson { ChangeId = id, Text = text }); await db.SaveChangesAsync(ct); return Back(id);
    }
    [HttpPost("/infra/changes/{id:guid}/lessons/{lessonId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, Guid lessonId, CancellationToken ct)
    {
        var lesson = await db.Lessons.SingleOrDefaultAsync(x => x.Id == lessonId && x.ChangeId == id, ct);
        if (lesson is null) return NotFound();
        lesson.Confirmed = true; await db.SaveChangesAsync(ct); return Back(id);
    }
    [HttpPost("/infra/changes/{id:guid}/analyses")]
    public async Task<IActionResult> Analyze(Guid id, CancellationToken ct)
    {
        try { await mediator.Send(new AnalyzeChange(id), ct); return Back(id); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or System.Text.Json.JsonException or TaskCanceledException or System.Collections.Generic.KeyNotFoundException or IndexOutOfRangeException)
        { TempData["AdvisorError"] = "Analysis unavailable. Check provider configuration, context budget, and retry."; return Back(id); }
    }
    [HttpGet("/infra/changes/{id:guid}/analyses/latest")]
    public async Task<IActionResult> Latest(Guid id, CancellationToken ct)
    {
        try { return Ok(await mediator.Send(new LatestAnalysis(id), ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message, isStale = true }); }
    }
    private IActionResult Back(Guid id) => Redirect($"/infra/changes/{id}/advisor");
    private static bool Valid(params string[] values) => values.All(x => !string.IsNullOrWhiteSpace(x) && x.Length <= 1500);
}
