# CarModels Demo (.NET 10 / C# 14)

This solution targets **.NET 10** and uses **C# 14 (preview)** features.

## Mutable vs. Immutable demo

The home page includes a small demo backed by `MutabilityDemo`:

- **Mutable class** (`MutableCar`) changes state in place.
- **Immutable record** (`ImmutableCar`) uses `with` to create a new copy.

## Run the demo

1. Make sure you have the .NET 10 SDK installed.
2. From the repository root, run:

```bash
dotnet restore
dotnet run --project WebApplication/WebApplication.csproj
```

3. Open the URL printed by the app (typically `https://localhost:5001` or `http://localhost:5000`).
4. Navigate to `/` and review the **Mutable vs. Immutable Demo** section.
