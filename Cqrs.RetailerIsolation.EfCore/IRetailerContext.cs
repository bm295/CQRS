namespace Cqrs.RetailerIsolation;

/// <summary>Provides the trusted retailer identity for the current unit of work.</summary>
public interface IRetailerContext
{
    Guid RetailerId { get; }
}
