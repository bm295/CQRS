namespace WebApplication.Domain;

/// <summary>
/// Marks data that must be isolated by retailer.
/// </summary>
public interface IBelongsToRetailer
{
    Guid RetailerId { get; }
}
