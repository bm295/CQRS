using Microsoft.EntityFrameworkCore;
using Cqrs.RetailerIsolation;
using WebApplication.Domain;

namespace WebApplication.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    private readonly IRetailerContext _retailerContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IRetailerContext retailerContext)
        : base(options)
    {
        _retailerContext = retailerContext;
    }

    public DbSet<InfrastructureChangeRequest> ChangeRequests => Set<InfrastructureChangeRequest>();

    private Guid CurrentRetailerId => _retailerContext.RetailerId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var changeRequest = modelBuilder.Entity<InfrastructureChangeRequest>();
        changeRequest.HasKey(x => x.Id);
        changeRequest.HasIndex(x => new { x.RetailerId, x.Status });
        changeRequest.HasQueryFilter(x => x.RetailerId == CurrentRetailerId);
    }
}
