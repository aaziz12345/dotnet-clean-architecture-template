# dotnet-clean-architecture-template

A production-grade .NET 8 template for building secure, testable, and scalable backend services using Clean Architecture.

Designed to separate business rules from infrastructure concerns, this repository demonstrates architecture discipline, operational readiness, and practical engineering standards expected in senior backend roles.

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

## Why This Project Exists

This template exists to accelerate delivery of real-world APIs without sacrificing architectural quality.

It solves common backend issues:
- Tight coupling between business logic and frameworks
- Low testability due to infrastructure-heavy code
- Inconsistent error handling and response contracts
- Weak deployment and operational setup

Clean Architecture matters because it enforces boundaries. Domain and application logic remain stable while infrastructure and delivery mechanisms evolve. This repository demonstrates production readiness through authentication, authorization, logging, exception handling, testing, Dockerization, migrations, and health monitoring.

## Architecture Overview

### Layers and Responsibilities

- `Domain`
  - Core entities, value concepts, and repository contracts
  - No external dependencies
- `Application`
  - Use cases, DTOs, validators, service orchestration, result pattern
  - Depends only on `Domain`
- `Infrastructure`
  - EF Core, SQL Server access, repository implementations, JWT/token services, hashing, seeding
  - Implements application/domain contracts
- `WebAPI`
  - HTTP endpoints, authentication setup, middleware, Swagger, health checks, routing, composition root

### Dependency Rule

Dependencies point inward:
- `WebAPI -> Infrastructure -> Application -> Domain`
- `Domain` never depends on outer layers

This rule keeps core business logic independent of frameworks and deployment concerns.

### Folder Structure

```text
dotnet-clean-architecture-template/
  src/
    Domain/
      Common/
      Entities/
      Enums/
      Repositories/
    Application/
      Common/
      DTOs/
      Interfaces/
      Services/
      Validators/
    Infrastructure/
      Options/
      Persistence/
      Repositories/
      Security/
    WebAPI/
      Controllers/
        APIControllers/
        ViewControllers/
      Middleware/
      Models/
      ViewModels/
      Views/
  tests/
    UnitTests/
  Dockerfile
  docker-compose.yml
  dotnet-clean-architecture-template.sln
```

## Key Features

- JWT authentication with secure token generation
- Role-based authorization (`Admin`, `User`) for protected operations
- Repository Pattern for persistence abstraction
- Layered dependency injection via `IServiceCollection` extension methods
- EF Core + SQL Server with migrations and startup seed data
- Result pattern for explicit success/failure flow
- Global exception middleware for consistent error responses
- Pagination support for product listing endpoints
- Serilog-based structured logging
- Health checks endpoint for runtime monitoring
- Swagger/OpenAPI documentation with Bearer token support
- Unit testing with xUnit + Moq covering core services
- Docker and Docker Compose support for local containerized execution

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit
- Moq
- Docker
- Serilog

## Getting Started (Local Development)

### 1. Clone the repository

```bash
git clone https://github.com/<your-org>/dotnet-clean-architecture-template.git
cd dotnet-clean-architecture-template
```

### 2. Configure settings

Update `src/WebAPI/appsettings.json` (or use `appsettings.Development.json`):
- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`
- `Jwt:Issuer`
- `Jwt:Audience`

### 3. Restore dependencies

```bash
dotnet restore dotnet-clean-architecture-template.sln
```

### 4. Run migrations

```bash
dotnet dotnet-ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/WebAPI/WebAPI.csproj
```

### 5. Run the API

```bash
dotnet run --project src/WebAPI/WebAPI.csproj
```

### 6. Access Swagger

Open:
- `http://localhost:5278/swagger` (Project profile)
- or your current Visual Studio launch URL

### 7. Test endpoints

Use Swagger or Postman for:
- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `GET/POST/PUT/DELETE /api/v1/products`

## Running with Docker

### Build image

```bash
docker build -t dotnet-clean-architecture-template .
```

### Run with Docker Compose

```bash
docker compose up --build
```

### Environment Variables

`docker-compose.yml` configures:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpiryMinutes`

### Access in containerized mode

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`

## Database Migration Guide

### Create a new migration

```bash
dotnet dotnet-ef migrations add <MigrationName> --project src/Infrastructure/Infrastructure.csproj --startup-project src/WebAPI/WebAPI.csproj --output-dir Persistence/Migrations
```

### Apply migrations

```bash
dotnet dotnet-ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/WebAPI/WebAPI.csproj
```

## Authentication Flow

1. Register user
   - `POST /api/v1/auth/register`
2. Login user
   - `POST /api/v1/auth/login`
3. Receive JWT token
   - token contains identity and role claims
4. Authorize requests
   - add header: `Authorization: Bearer <token>`

### Use token in Swagger

- Click `Authorize` in Swagger UI
- Enter `Bearer <token>`
- Call protected endpoints (products CRUD based on role)

## Running Unit Tests

Run all unit tests:

```bash
dotnet test tests/UnitTests/UnitTests.csproj -c Release
```

Coverage focus:
- `UserService`: registration/login success and failure paths
- `ProductService`: create/read/update/delete + pagination and validation scenarios
- Mocked repository behavior via Moq for isolated business logic testing

## Deployment Guide

### Option A: Azure App Service

1. Provision Azure SQL Database and App Service.
2. Set production app settings (connection string, JWT config, environment).
3. Deploy WebAPI artifact (GitHub Actions or zip deploy).
4. Run migrations on deployment/startup pipeline.
5. Configure monitoring and alerts via Azure Monitor/Application Insights.

### Option B: Docker on VPS

1. Provision Linux VM and install Docker + Compose.
2. Pull/build the image on server.
3. Configure environment variables securely (`.env`/secret store).
4. Run with `docker compose up -d` behind Nginx/Traefik.
5. Enable HTTPS, log shipping, and backup strategy for SQL Server.

### Option C: CI/CD Pipeline (GitHub Actions)

High-level pipeline:
1. Trigger on pull request and main branch updates.
2. Restore, build, test.
3. Build Docker image and push to container registry.
4. Deploy to target environment.
5. Run post-deployment migration step and health check validation.

## Production Readiness Highlights

This project demonstrates:

- Architecture thinking
  - Clear layer boundaries and dependency direction
- Separation of concerns
  - Domain and application are isolated from delivery and persistence
- Testability
  - Business services tested with mocked dependencies
- Scalability
  - Extensible structure for new modules and infrastructure adapters
- Maintainability
  - Consistent patterns, validation, DI registration, and code organization
- Security best practices
  - JWT authentication, role checks, password hashing, centralized exception handling

## Future Improvements

- Add distributed caching (Redis)
- Introduce background jobs (Hangfire/Quartz)
- Implement refresh tokens and token revocation
- Add rate limiting and API throttling policies
- Integrate OpenTelemetry tracing and metrics
- Add integration and contract tests

## Screenshots

- `Swagger UI`: `[Add screenshot here]`
- `Folder Structure`: `[Add screenshot here]`
- `Docker Running Containers`: `[Add screenshot here]`

## License

MIT License (placeholder).

If this repository is used publicly, add a `LICENSE` file with the full MIT text.
