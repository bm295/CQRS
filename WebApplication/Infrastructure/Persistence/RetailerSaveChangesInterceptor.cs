using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication.Domain;

namespace WebApplication.Infrastructure.Persistence;

/// <summary>
/// Assigns the current retailer on inserts and prevents cross-retailer writes.
/// Query isolation is handled by the global query filter in <see cref="ApplicationDbContext"/>.
/// </summary>
public sealed class RetailerSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IRetailerContext _retailerContext;

    public RetailerSaveChangesInterceptor(IRetailerContext retailerContext)
    {
        _retailerContext = retailerContext;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyRetailerRules(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyRetailerRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyRetailerRules(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var retailerId = _retailerContext.RetailerId;
        if (retailerId == Guid.Empty)
        {
            throw new InvalidOperationException("A valid retailer is required for this unit of work.");
        }

        foreach (var entry in dbContext.ChangeTracker.Entries<IBelongsToRetailer>())
        {
            var property = entry.Property(nameof(IBelongsToRetailer.RetailerId));

            if (entry.State == EntityState.Added)
            {
                if ((Guid)property.CurrentValue! == Guid.Empty)
                {
                    property.CurrentValue = retailerId;
                }
                else if ((Guid)property.CurrentValue != retailerId)
                {
                    throw new UnauthorizedAccessException("Cannot create data for another retailer.");
                }

                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                if ((Guid)property.OriginalValue! != retailerId)
                {
                    throw new UnauthorizedAccessException("Cannot modify data owned by another retailer.");
                }

                if (entry.State == EntityState.Modified && (Guid)property.CurrentValue! != retailerId)
                {
                    throw new UnauthorizedAccessException("Retailer ownership cannot be changed.");
                }

                property.IsModified = false;
            }
        }
    }
}
