# Story 01 — User Story 1 — Initialize Cinema Reservation API (Story: 1)

## Prerequisites
None. Initial story in the `project-init-infra` feature.

## Story Goal
Initialize the Cinema Ticket Reservation API backend with Entity Framework Core, SQL Server database integration, EF Core Code-First migrations, global exception handling, health checks, structured logging, and Swagger/OpenAPI support on ASP.NET Core 8.0.

## Context — Read These Files First
1. `cinema-tickets-back.csproj` — lines 1–15. Currently targets `net8.0` with `Swashbuckle.AspNetCore` 6.6.2. Needs NuGet packages for EF Core SQL Server and Tools (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`).
2. `Program.cs` — lines 1–22. Minimal API bootstrap. Currently configures Swagger in Dev, maps sample endpoint and application endpoints. Needs EF Core DbContext registration, exception handling middleware, and health check endpoints.
3. `appsettings.json` — lines 1–13. Contains `Logging` config and `ConnectionStrings:DefaultConnection`.
4. `appsettings.Development.json` — lines 1–9. Environment configuration for Development.
5. `Extensions/ServiceCollectionExtensions.cs` — lines 1–13. Configures DI (`AddApplicationServices`). Needs `AddDbContext<AppDbContext>` and `AddHealthChecks()`.
6. `Extensions/EndpointRouteBuilderExtensions.cs` — lines 1–13. Maps endpoints (`MapApplicationEndpoints`). Needs `MapHealthChecks("/health")`.
7. `Models/Movie.cs` — lines 1–12. Domain entity `Movie`. Will be configured in `AppDbContext`.

## Product rules (from story)
- **Current behaviour**: Application is a skeleton Minimal API with in-memory `MovieEndpoints` list and simple `SqlServerConnectionFactory`.
- **New behaviour**: Application connects to SQL Server via EF Core `AppDbContext`, supports Code First migrations, handles unhandled exceptions globally with standard API error payload, and exposes `/health` endpoint for database and API availability checks.

## Implementation body

### 1 — Add EF Core NuGet Packages
File: `cinema-tickets-back.csproj`
Add the following package references under `<ItemGroup>`:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.8">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.8" />
```

### 2 — Create AppDbContext
Create file: `Infrastructure/Database/AppDbContext.cs`
Define EF Core DbContext class mapping `Movie` entity:
```csharp
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
    }
}
```

### 3 — Create Global Exception Handling Middleware
Create file: `Middleware/ExceptionHandlingMiddleware.cs`
Catches unhandled exceptions and returns uniform JSON error response with standard structure:
```csharp
using System.Net;
using System.Text.Json;

namespace CinemaTicketsBack.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing.");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            status = context.Response.StatusCode,
            message = "An internal server error occurred. Please try again later.",
            detail = exception.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### 4 — Register DbContext and Services in DI
File: `Extensions/ServiceCollectionExtensions.cs`
Update `AddApplicationServices` to register `AppDbContext` using SQL Server connection string and configure Health Checks:
```csharp
using CinemaTicketsBack.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IDatabaseConnectionFactory, SqlServerConnectionFactory>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database_health_check");

        return services;
    }
}
```

### 5 — Map Health Check Endpoints
File: `Extensions/EndpointRouteBuilderExtensions.cs`
Update `MapApplicationEndpoints` to map `/health` endpoint:
```csharp
using CinemaTicketsBack.Features.Movies;

namespace CinemaTicketsBack.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        app.MapHealthChecks("/health");
        return app;
    }
}
```

### 6 — Configure Exception Middleware and Logging in Program.cs
File: `Program.cs`
Register `ExceptionHandlingMiddleware` and verify Swagger configuration:
```csharp
using CinemaTicketsBack.Extensions;
using CinemaTicketsBack.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { message = "Cinema Tickets API is running." }));
app.MapApplicationEndpoints();

app.Run();
```

### 7 — Create Initial EF Core Migration
Execute `dotnet ef migrations add InitialCreate` in the root directory to generate the initial database migration under `Migrations/`.

---

## Edge Cases & Failure Modes
- **Missing or Invalid Connection String**: Triggered when `ConnectionStrings:DefaultConnection` is missing in `appsettings.json`. Enforced in `Extensions/ServiceCollectionExtensions.cs` lines 10-11 by throwing `InvalidOperationException`.
- **SQL Server Connection Timeout / Server Offline**: Triggered during app execution or `/health` check when SQL Server database is unreachable. Enforced in EF Core connection pipeline and reported via `/health` returning HTTP 503 Service Unavailable.
- **Unhandled Exceptions in Endpoints**: Triggered when any endpoint throws an unhandled exception. Enforced in `Middleware/ExceptionHandlingMiddleware.cs` catching `Exception`, logging structured error log, and returning HTTP 500 JSON payload.
- **Environment Specific Configs**: Triggered when running in `Production`. Enforced in `Program.cs` lines 14-17 where Swagger UI middleware is restricted to `Development` environment (`app.Environment.IsDevelopment()`).

---

## Test Plan
1. **Unit Test - Exception Handling Middleware**:
   File: `Tests/Middleware/ExceptionHandlingMiddlewareTests.cs`
   - `InvokeAsync_WhenExceptionThrown_Returns500JsonPayload`: Verify that unhandled exceptions produce HTTP 500 status code and expected JSON body.
2. **Integration Test - Database Context**:
   File: `Tests/Infrastructure/AppDbContextTests.cs`
   - `CanAddAndRetrieveMovie`: Test CRUD operations using In-Memory or SQL Server local DB.
3. **Smoke Test - Health Check Endpoint**:
   File: `Tests/Endpoints/HealthCheckEndpointTests.cs`
   - `GET /health` returns HTTP 200 OK when database is reachable.

---

## Migration / Rollback
- **Migration**: Run `dotnet ef migrations add InitialCreate` to create `Migrations/` directory and InitialCreate snapshot. Run `dotnet ef database update` to apply migrations to SQL Server.
- **Rollback**: To revert database migration, run `dotnet ef database update 0` and remove `Migrations/` directory.

---

## Verification Steps
1. **Backend builds:** Run `dotnet build` in root directory `.`.
2. **EF Core migration creation:** Run `dotnet ef migrations add InitialCreate` in root directory `.`.
3. **Application start & smoke test:** Run `dotnet run` in root directory `.`, verify console output starts without error.
4. **Health check endpoint verification:** Execute `curl -i http://localhost:5000/health` (or configured port) and verify HTTP 200 OK.
5. **Swagger UI verification:** Open `http://localhost:5000/swagger` in browser during development.

---

## Done Criteria
- [ ] ASP.NET Core Web API project compiles cleanly with EF Core SQL Server packages.
- [ ] `AppDbContext` configured with `Movie` entity mapping.
- [ ] `appsettings.json` configured with SQL Server connection string (no hardcoded plain credentials).
- [ ] Initial EF Core migration (`InitialCreate`) can be created and applied.
- [ ] Swagger/OpenAPI enabled and accessible in Development environment.
- [ ] Health check endpoint `/health` registered and returns status.
- [ ] Global `ExceptionHandlingMiddleware` catches unhandled exceptions and returns standardized JSON error response.
- [ ] Structured logging configured via ASP.NET Core Logging infrastructure.
