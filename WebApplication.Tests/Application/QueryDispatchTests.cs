using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WebApplication.Application.Queries;
using WebApplication.Infrastructure.ReadModels;
namespace WebApplication.Tests.Application;
public sealed class QueryDispatchTests
{
    [Test] public async Task All_operational_queries_dispatch_through_mediator()
    {
        var services = new ServiceCollection();
        services.AddMediatR(typeof(GetChangeRequestByIdQuery).Assembly);
        services.AddSingleton<IChangeReadModelStore, InMemoryChangeReadModelStore>();
        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        Assert.That(await mediator.Send(new GetChangeRequestByIdQuery(Guid.NewGuid())), Is.Null);
        Assert.That(await mediator.Send(new ListPendingApprovalsQuery()), Is.Empty);
        Assert.That(await mediator.Send(new ListScheduledChangesQuery()), Is.Empty);
        Assert.That(await mediator.Send(new ListFailedChangesQuery()), Is.Empty);
        Assert.That(await mediator.Send(new GetOperationalChangeSummaryQuery()), Is.Not.Null);
    }
}
