using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Cqrs.RetailerIsolation;

/// <summary>
/// Assigns the current retailer on inserts and blocks cross-retailer updates and deletes.
/// Configure a matching EF Core query filter in each consuming DbContext.
/// </summary>
public sealed class RetailerSaveChangesInterceptor(IRetailerContext retailerContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
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
        if (dbContext is null) return;

        var retailerId = retailerContext.RetailerId;
        if (retailerId == Guid.Empty)
            throw new InvalidOperationException("A valid retailer is required for this unit of work.");

        foreach (var entry in dbContext.ChangeTracker.Entries<IBelongsToRetailer>())
        {
            var property = entry.Property(nameof(IBelongsToRetailer.RetailerId));

            if (entry.State == EntityState.Added)
            {
                if ((Guid)property.CurrentValue! == Guid.Empty) property.CurrentValue = retailerId;
                else if ((Guid)property.CurrentValue! != retailerId)
                    throw new UnauthorizedAccessException("Cannot create data for another retailer.");
                continue;
            }

            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                if ((Guid)property.OriginalValue! != retailerId)
                    throw new UnauthorizedAccessException("Cannot modify data owned by another retailer.");
                if (entry.State == EntityState.Modified && (Guid)property.CurrentValue! != retailerId)
                    throw new UnauthorizedAccessException("Retailer ownership cannot be changed.");
                property.IsModified = false;
            }
        }
    }
}
