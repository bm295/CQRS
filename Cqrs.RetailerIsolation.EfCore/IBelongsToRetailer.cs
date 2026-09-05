namespace Cqrs.RetailerIsolation;

/// <summary>Marks an entity as owned by a retailer.</summary>
public interface IBelongsToRetailer
{
    Guid RetailerId { get; }
}
