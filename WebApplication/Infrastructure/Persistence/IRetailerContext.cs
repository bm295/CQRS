namespace WebApplication.Infrastructure.Persistence;

/// <summary>
/// Provides the retailer for the current unit of work from a trusted identity source.
/// </summary>
public interface IRetailerContext
{
    Guid RetailerId { get; }
}
