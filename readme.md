# Infrastructure Change CQRS Web App

This solution implements the MVP from `docs/cqrs-simple-domain-plan.md` as an ASP.NET Core MVC web application with clear CQRS separation:

- Write side: `Domain`, `Application/Commands`, `Infrastructure/Persistence`
- Read side: `Application/Queries`, `Infrastructure/ReadModels`
- Delivery: command/query HTTP routes plus Razor UI pages for dashboard, approvals, schedule, failures, create, and detail

## Run

```bash
dotnet restore
dotnet run --project WebApplication/WebApplication.csproj
```

Open `/infra/dashboard`.

## Reusable DLL

`Cqrs.RetailerIsolation.EfCore` is a versioned, independent .NET 10 library that protects retailer-owned EF Core entities during `SaveChanges`.

Build the DLL consumed by ApiDemo with:

```bash
dotnet build Cqrs.RetailerIsolation.EfCore/Cqrs.RetailerIsolation.EfCore.csproj --configuration Release --output artifacts/lib/net10.0
```

The current assembly version is `1.0.0.0`. A consuming application must implement `IRetailerContext`, make its entities implement `IBelongsToRetailer`, register `RetailerSaveChangesInterceptor`, and configure a retailer query filter.

## HTTP Endpoints

Commands:

- `POST /infra/changes`
- `POST /infra/changes/{id}/submit`
- `POST /infra/changes/{id}/approve`
- `POST /infra/changes/{id}/reject`
- `POST /infra/changes/{id}/schedule`
- `POST /infra/changes/{id}/start`
- `POST /infra/changes/{id}/complete`
- `POST /infra/changes/{id}/fail`

Queries:

- `GET /infra/changes/{id}`
- `GET /infra/approvals/pending`
- `GET /infra/changes/scheduled`
- `GET /infra/changes/failed`
- `GET /infra/changes/summary`

## Notes

- The write repository and projection store are both in-memory for the MVP.
- Read pages are backed by projected read models, not the aggregate repository.
- Automated tests cover aggregate transitions and a command/query workflow over the in-memory stores.
