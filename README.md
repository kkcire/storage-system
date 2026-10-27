# Storage System

A storage management application developed as a practical study project while learning C# and .NET.

## About

This is an educational project built to apply concepts learned while studying C# and .NET: relational database modeling, object-relational mapping with Entity Framework Core, layered architecture, and rich domain modeling.

The application manages **brands** and **products** (with stock tracking) through a console interface built with [Spectre.Console](https://spectreconsole.net/). The project's main focus is combining these concepts in a single application while developing a better understanding of application structure, data persistence, and business rules.

## Features

- Brand registration, update, deletion, and search
- Product registration, update, deletion, and search
- Products linked to brands (1:N relationship)
- Stock quantity tracking with safe adjustments
- Duplicate-name prevention at both domain and database levels
- Business-rule validation centralized in the domain
- Rich domain model with static factories and private setters
- Persistence with Entity Framework Core and SQL Server
- Unit and integration tests with xUnit

## Tech Stack

- C#
- .NET 10
- ASP.NET Core (future Web API layer)
- Entity Framework Core 10
- SQL Server (application) · SQLite in-memory (tests)
- xUnit
- Spectre.Console

## Architecture

The solution follows a layered structure, with dependencies pointing inward:

```text
StorageSystem/
├── src/
│   ├── StorageSystem.Domain/          # Entities, guards, domain exceptions
│   │   ├── Entities/
│   │   ├── Exceptions/
│   │   └── Guards/
│   │
│   ├── StorageSystem.Application/     # Use cases and repository contracts
│   │   ├── Repository/                # IBrandRepository, IProductRepository
│   │   └── Services/                  # BrandService, ProductService
│   │
│   ├── StorageSystem.Infrastructure/  # EF Core persistence
│   │   ├── Data/                      # StorageContext, factory
│   │   ├── Repository/                # Repository implementations
│   │   └── Migrations/
│   │
│   └── StorageSystem.Console/         # Spectre.Console UI
│       └── UI/
│
└── test/
    └── StorageSystem.Tests/
        ├── Domain/                    # Pure unit tests (no I/O)
        ├── Application/               # Service tests (SQLite in-memory)
        └── Infrastructure/            # Repository and constraint tests
```

**Dependency direction:** `Console → Application → Domain` and `Infrastructure → Application → Domain`. The Domain project has no framework dependencies.

### Domain model highlights

- **Rich entities.** `Brand` and `Product` have private setters, a private parameterless constructor for EF Core, and static factories (`Create`) that enforce invariants. Mutations happen through explicit methods (`Rename`, `ChangePrice`, `AddStock`, `ChangeBrand`).
- **Guards.** `GuardClause` centralizes validations such as null/whitespace names, negative prices, negative quantities, and maximum name length.
- **Domain exceptions.** `DomainException` represents business-rule violations, keeping infrastructure and HTTP concerns out of the domain.
- **Name limits.** `Brand.MaxNameLength` is 50 and `Product.MaxNameLength` is 100, enforced by both the guard and the database schema.
- **Database constraints.** Unique indexes on `Brand.Name` and `Product.Name`, `decimal(18,2)` precision for `Product.Price`, and `DeleteBehavior.Restrict` on the product–brand foreign key.

## What I Learned

This project was a great opportunity to apply theoretical concepts in a single, practical application. Main takeaways include:

- Data persistence, object-relational mapping, and managing database relationships with EF Core and SQL Server.
- Designing a rich domain model with invariants protected inside the entities.
- Applying the Repository pattern and Dependency Inversion to decouple use cases from persistence.
- Separating responsibilities across Domain, Application, Infrastructure, and UI layers.
- Dependency injection and configuration management (appsettings, environment variables, user secrets).
- Writing unit tests for the domain and integration tests against an in-memory SQLite database.
- Using EF Core migrations and design-time factories to keep the schema versioned.

## Getting Started

### Requirements

- .NET 10 SDK or later
- SQL Server (Docker, LocalDB, or a local instance)

If you use Docker, a SQL Server container can be started with:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 -d --name sqlserver \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Clone the repository

```bash
git clone https://github.com/kkcire/storage-system.git
cd storage-system
```

### Configure the connection string

The connection string is read from the following sources, in order:

1. `src/StorageSystem.Console/appsettings.json` (`ConnectionStrings:DefaultConnection`)
2. `src/StorageSystem.Console/appsettings.Development.json`
3. Environment variable `ConnectionStrings__DefaultConnection`
4. User Secrets

> **Security note:** the repository ships a development-only connection string in `appsettings.json` for convenience. For any non-local environment, use user secrets or environment variables and never commit real credentials.

### Apply database migrations

```bash
dotnet ef database update \
  --project src/StorageSystem.Infrastructure \
  --startup-project src/StorageSystem.Console
```

### Run

```bash
dotnet run --project src/StorageSystem.Console
```

### Run the tests

```bash
dotnet test
```

> Integration tests use SQLite in-memory databases, so they do not require SQL Server, LocalDB, Docker, or any machine-specific connection string. The application itself uses SQL Server through the Infrastructure layer.
