# DroneBuilder

A .NET 9 Web API for configuring and managing drone builds.

## Projects

| Project | Description |
|---|---|
| `DroneBuilder.API` | ASP.NET Core Web API — endpoints, middleware, auth |
| `DroneBuilder.Application` | Application layer — use cases, CQRS handlers |
| `DroneBuilder.Domain` | Domain entities and interfaces |
| `DroneBuilder.Infrastructure` | EF Core, persistence, external services |
| `DroneBuilder.Application.Tests` | Unit tests |

## Getting Started

```bash
# Restore & build
dotnet restore ./DroneBuilder/DroneBuilder.sln
dotnet build   ./DroneBuilder/DroneBuilder.sln

# Run tests
dotnet test ./DroneBuilder/DroneBuilder.sln
```

## Code Quality

The repository enforces C# formatting rules defined in [`DroneBuilder/.editorconfig`](./DroneBuilder/.editorconfig).  
Every pull request is automatically checked by the **CI - Format Check** workflow.

### Check formatting (mirrors CI — fails on violations)

```bash
dotnet format ./DroneBuilder/DroneBuilder.sln --verify-no-changes --verbosity diagnostic
```

### Fix formatting locally

```bash
dotnet format ./DroneBuilder/DroneBuilder.sln
```

Run the fix command before pushing to avoid a failed CI check.