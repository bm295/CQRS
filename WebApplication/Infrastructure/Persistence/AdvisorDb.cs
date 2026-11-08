using Microsoft.EntityFrameworkCore;
using WebApplication.Domain;
using WebApplication.Application.AI;
namespace WebApplication.Infrastructure.Persistence;
public sealed class AdvisorDb(DbContextOptions<AdvisorDb> options) : DbContext(options)
{
    public DbSet<InfrastructureChangeRequest> Changes => Set<InfrastructureChangeRequest>();
    public DbSet<Knowledge> Knowledge => Set<Knowledge>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<ChangePlan> Plans => Set<ChangePlan>();
    public DbSet<Analysis> Analyses => Set<Analysis>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<InfrastructureChangeRequest>().HasKey(x => x.Id);
        b.Entity<ChangePlan>().HasKey(x => x.ChangeId);
    }
}
public sealed class SqliteChangeRepository(AdvisorDb db) : IInfrastructureChangeRequestRepository
{
    public async Task AddAsync(InfrastructureChangeRequest change, CancellationToken ct = default)
    { db.Changes.Add(change); await db.SaveChangesAsync(ct); }
    public Task<InfrastructureChangeRequest?> GetAsync(Guid id, CancellationToken ct = default) => db.Changes.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task UpdateAsync(InfrastructureChangeRequest change, CancellationToken ct = default)
    { db.Changes.Update(change); await db.SaveChangesAsync(ct); }
}
