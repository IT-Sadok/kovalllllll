# DroneBuilder

An online shop for FPV drone parts with a quad builder. You pick a frame, motors, props, electronics, battery and video gear, and the builder checks as you go that the parts fit together, then works out weight and thrust to weight. A finished build can be saved to your account or sent to the cart in one click.

The catalog is filled from [RaceDayQuads](https://www.racedayquads.com) by an admin-triggered import. Every product links back to its source listing.

Built with .NET 9 (ASP.NET Core Minimal APIs, EF Core, PostgreSQL, RabbitMQ) and React 19 (Vite, TanStack Query, Tailwind CSS).

## Features

### For customers
- **Catalog** with search, brand, price, stock and sorting filters. When you pick a category, spec filters appear for it: KV, cell count, mounting, prop size, battery capacity, video system, connectors and radio protocol. Product variants such as colours or KV options show up as one card with a variant picker.
- **Builder** (`/builder`):
  - One slot per part: frame, motors, props, stack or FC + ESC, receiver, battery, camera, VTX, antenna and radio.
  - The part picker can show only the parts that fit what's already chosen.
  - A live summary shows price, dry and take-off weight, thrust to weight, and every compatibility issue.
  - The build is kept in the browser until you save it.
- **Saved builds** (`/builds`): save, rename, reopen, delete, or add a saved build to the cart.
- **Cart and checkout**:
  - Stock is reserved when a part goes into the cart and released when the reservation expires.
  - A whole build is reserved in one step: all parts or none.
  - Payment runs through Stripe Checkout.
- **Accounts**: registration with email confirmation (Resend), cookie-based JWT sign-in, and order history.

### For admins
- Products: create, edit, delist and restore, with a fixed category, manufacturer, weight, free-form attributes, images (Azure Blob Storage) and a typed component spec.
- Filters for "needs review" (imported parts whose spec could not be parsed) and by category.
- Warehouse: stock per product, plus a one-click restock of every empty item.
- **RaceDayQuads import**: runs in the background and reports how many products were added, updated or flagged for review. Running it again updates existing products instead of duplicating them.
- Order management with status transitions.

### Compatibility rules
Errors mean the part will not work. Warnings and notes flag something worth a second look. Nothing blocks checkout.

| Rule | Severity |
|---|---|
| Required parts: frame, 4 motors, props, battery, and FC + ESC or a stack | Error |
| Prop larger than the frame allows (0.2" tolerance) | Error |
| Prop more than 1" smaller than the frame is built for | Warning |
| FC / ESC / stack / motor mounting does not match the frame | Error |
| Battery cell count outside what motors, ESC or FC accept | Error |
| Camera and VTX on different video systems | Error |
| Radio and receiver on different protocols | Error |
| Prop hub does not match the motor shaft | Error |
| Camera width does not match the frame | Warning |
| ESC current rating below motor draw + 20% | Warning |
| Battery plug differs from the ESC lead | Warning |
| VTX and antenna connectors differ | Warning |
| KV × cells × prop size outside the usual range | Warning |
| Part has no spec yet | Info |
| Take-off weight above 250 g | Info |

## Architecture

```
├── client/                      React + Vite SPA
├── server/
│   ├── src/
│   │   ├── DroneBuilder.API             Minimal API endpoints, auth, middleware, OpenAPI
│   │   ├── DroneBuilder.Application     Commands and queries, validation, compatibility rules, import parsing
│   │   ├── DroneBuilder.Domain          Entities, component specs, domain events
│   │   └── DroneBuilder.Infrastructure  EF Core (PostgreSQL), repositories, RabbitMQ, Azure Blob, Stripe, Resend, importer
│   ├── tests/DroneBuilder.Application.Tests   xUnit + NSubstitute
│   └── DroneBuilder.sln
├── docker-compose.yml           Local stack: PostgreSQL, RabbitMQ, API
└── Dockerfile                   One image: the client build is served by the API
```

- **Clean architecture:** a small in-house mediator dispatches commands and queries. Validation uses FluentValidation, and errors are returned through FluentResults in a common `ApiResponse` envelope.
- **Component specs:** one EF Core TPH table (`ComponentSpecs`), one subclass per part type. The product category decides which spec a product can have.
- **Events:** domain events go to an outbox table in the same transaction. A background service publishes them to RabbitMQ.
- **Background services:** outbox publishing, event consumers, expiry of cart reservations, and the catalog import queue.

## Getting started

### Prerequisites
- Docker with Compose
- .NET SDK 9.0.300 or newer, only for running the API outside Docker
- Node.js 20 or newer, only for running the client outside Docker

### Run everything with Docker

```bash
cp .env.example .env
```

Fill in `.env` (see [Configuration](#configuration)), then:

```bash
docker compose up --build
```

- App: http://localhost:8080
- API reference (Scalar, Development only): http://localhost:8080/scalar/v1
- RabbitMQ management: http://localhost:15672

Migrations run on start, and the admin and user accounts from `.env` are seeded. To get a catalog:
1. Sign in as the admin.
2. Open **Import** and start the RaceDayQuads import.
3. Open **Warehouse** and use **Restock** to give the imported parts some stock.

### Run the API and client locally

Start only the infrastructure in Docker:

```bash
docker compose up -d db rabbitmq
```

Create `server/src/DroneBuilder.API/appsettings.Development.json`. It is git-ignored. Use the shape of `appsettings.json`, fill in your connection string, JWT, Azure Storage, Resend and Stripe values, and set `DatabaseInitialization` to apply migrations and seed accounts. Then:

```bash
cd server
dotnet run --project src/DroneBuilder.API
```

The API listens on http://localhost:5037.

```bash
cd client
npm ci
npm run dev
```

The client runs on http://localhost:5173 and proxies `/api` to the API.

## Configuration

`docker-compose.yml` reads these from `.env`. On other hosts, set them as environment variables, using `__` for nesting (for example `JwtOptions__Key`).

| Variable | Purpose |
|---|---|
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB` | Database credentials |
| `RABBITMQ_USER`, `RABBITMQ_PASSWORD` | Message broker credentials |
| `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_KEY`, `JWT_EXPIRY_MINUTES` | Token signing for the auth cookie |
| `AZURE_STORAGE_CONNECTION_STRING`, `AZURE_STORAGE_CONTAINER` | Product image uploads |
| `RESEND_API_KEY`, `RESEND_FROM_EMAIL`, `RESEND_CONFIRMATION_URL` | Confirmation emails |
| `PAYMENT_PROVIDER` | `Stripe`, or `Fake` for local testing. `Fake` marks orders paid without charging, so never use it in production. |
| `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET`, `STRIPE_CURRENCY`, `STRIPE_SUCCESS_URL`, `STRIPE_CANCEL_URL` | Stripe Checkout |
| `IDENTITY_ADMIN_EMAIL`, `IDENTITY_ADMIN_PASSWORD`, `IDENTITY_USER_EMAIL`, `IDENTITY_USER_PASSWORD` | Seeded accounts |

Other settings live in `server/src/DroneBuilder.API/appsettings.json`:

| Section | Purpose |
|---|---|
| `DatabaseInitialization` | `ApplyMigrations` and `SeedIdentity`. Both are off by default outside Development. |
| `RaceDayQuadsImport` | Source URL, products per category, variants per product, and the delay between requests |
| `CartReservation` | How long cart stock stays reserved and how often expired reservations are released |
| `RateLimiting` | Limits for sign-in and email endpoints |
| `Cors`, `ForwardedHeaders` | Needed when the client and API run on different origins or behind a proxy |

## Tests and code quality

```bash
cd server
dotnet test DroneBuilder.sln
dotnet format DroneBuilder.sln --verify-no-changes
```

To fix formatting:

```bash
cd server
dotnet format DroneBuilder.sln
```

```bash
cd client
npm run lint
npm run build
```

C# style rules live in [`server/.editorconfig`](./server/.editorconfig).

## CI/CD and branches

- `develop` is the integration branch. Feature branches open pull requests into it, and `develop` is merged into `main` for a release.
- **CI - Build and Test**: on pushes and pull requests to `main` and `develop`, builds and tests the server, then lints and builds the client.
- **CI - Format Check**: on pull requests to `main` and `develop`, runs `dotnet format --verify-no-changes`.
- **CD - Docker Deploy**: on pushes to `main`, builds the image, pushes it to GitHub Container Registry and deploys it to the Azure Web App.
  - The deploy does not run migrations by itself. They are applied on start only when `DatabaseInitialization__ApplyMigrations=true` is set for the app. Otherwise, apply them with `dotnet ef database update` or a script from `dotnet ef migrations script`.
