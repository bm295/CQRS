using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WebApplication.Domain;
using WebApplication.Infrastructure.Persistence;

namespace WebApplication.Tests.Infrastructure;

[TestFixture]
public sealed class RetailerIsolationTests
{
    [Test]
    public async Task Query_filter_only_returns_the_current_retailers_data()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = CreateOptions(connection);
        var retailerA = new MutableRetailerContext(Guid.NewGuid());
        var retailerB = new MutableRetailerContext(Guid.NewGuid());

        await using (var setup = CreateContext(options, retailerA))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Add(CreateRequest("Retailer A request"));
            await setup.SaveChangesAsync();
        }

        await using (var setup = CreateContext(options, retailerB))
        {
            setup.Add(CreateRequest("Retailer B request"));
            await setup.SaveChangesAsync();
        }

        await using var context = CreateContext(options, retailerA);
        var visible = await context.ChangeRequests.ToListAsync();

        Assert.That(visible, Has.Count.EqualTo(1));
        Assert.That(visible[0].Title, Is.EqualTo("Retailer A request"));
        Assert.That(visible[0].RetailerId, Is.EqualTo(retailerA.RetailerId));
    }

    [Test]
    public async Task Interceptor_rejects_changing_retailer_ownership()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var retailer = new MutableRetailerContext(Guid.NewGuid());
        await using var context = CreateContext(CreateOptions(connection), retailer);
        await context.Database.EnsureCreatedAsync();
        var request = CreateRequest("Protected request");
        context.Add(request);
        await context.SaveChangesAsync();

        context.Entry(request).Property(x => x.RetailerId).CurrentValue = Guid.NewGuid();

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => context.SaveChangesAsync());
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions(SqliteConnection connection) =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

    private static ApplicationDbContext CreateContext(
        DbContextOptions<ApplicationDbContext> options,
        IRetailerContext retailerContext)
    {
        var interceptor = new RetailerSaveChangesInterceptor(retailerContext);
        var optionsWithInterceptor = new DbContextOptionsBuilder<ApplicationDbContext>(options)
            .AddInterceptors(interceptor)
            .Options;
        return new ApplicationDbContext(optionsWithInterceptor, retailerContext);
    }

    private static InfrastructureChangeRequest CreateRequest(string title) =>
        InfrastructureChangeRequest.Create(title, "payments", "prod", "operator", DateTime.UtcNow);

    private sealed class MutableRetailerContext(Guid retailerId) : IRetailerContext
    {
        public Guid RetailerId { get; } = retailerId;
    }
}
