# Cqrs.RetailerIsolation.EfCore

`Cqrs.RetailerIsolation.EfCore` provides EF Core write-side tenant isolation.

## Installation

```xml
<PackageReference Include="Cqrs.RetailerIsolation.EfCore" Version="1.0.0" />
```

Implement `IRetailerContext` from a trusted request identity, make retailer-owned entities implement `IBelongsToRetailer`, and register `RetailerSaveChangesInterceptor` with EF Core. Consumers must also configure a global query filter for their entity types.
