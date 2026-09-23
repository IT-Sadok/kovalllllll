# DroneBuilder

A .NET 9 Web API with a React client for configuring and managing drone builds.

## Repository Structure

```
├── server/                 .NET backend
│   ├── src/                application projects
│   ├── tests/              test projects
│   └── DroneBuilder.sln
├── client/                 React + Vite frontend
├── docker-compose.yml      local stack (PostgreSQL, RabbitMQ, API)
└── Dockerfile              single image: client build served by the API
```

## Projects

| Project | Description |
|---|---|
| `server/src/DroneBuilder.API` | ASP.NET Core Web API — endpoints, middleware, auth |
| `server/src/DroneBuilder.Application` | Application layer — use cases, CQRS handlers |
| `server/src/DroneBuilder.Domain` | Domain entities and interfaces |
| `server/src/DroneBuilder.Infrastructure` | EF Core, persistence, external services |
| `server/tests/DroneBuilder.Application.Tests` | Unit tests |

## Getting Started

### Server

```bash
cd server

# Restore & build
dotnet restore DroneBuilder.sln
dotnet build   DroneBuilder.sln

# Run tests
dotnet test DroneBuilder.sln
```

### Client

```bash
cd client
npm ci
npm run dev
```

### Docker

```bash
docker compose up --build
```

## Code Quality

The repository enforces C# formatting rules defined in [`server/.editorconfig`](./server/.editorconfig).  
Every pull request is automatically checked by the **CI - Format Check** workflow.

### Check formatting (mirrors CI — fails on violations)

```bash
cd server
dotnet format DroneBuilder.sln --verify-no-changes --verbosity diagnostic
```

### Fix formatting locally

```bash
cd server
dotnet format DroneBuilder.sln
```

Run the fix command before pushing to avoid a failed CI check.
