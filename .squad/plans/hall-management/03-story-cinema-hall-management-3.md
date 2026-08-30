# Story 03 — User Story 3 — Cinema Hall Management (Story: 3)

## Prerequisites
- [Story 01 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/project-init-infra/01-story-initialize-cinema-reservation-api-1.md): ASP.NET Core 8.0 API project initialized with EF Core `AppDbContext` and SQL Server infrastructure.
- [Story 02 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/users-auth/02-story-user-registration-and-authentication-2.md): User authentication, JWT Bearer configuration, and `AdminOnly` authorization policy registered in DI pipeline.

## Story Goal
Implement domain entities, DTOs, EF Core mappings, and Minimal API CRUD & soft-deactivation endpoints (`/api/v1/halls`) for managing cinema halls and seating capacity restricted to supported hall types (`Standard`, `4D`, `Gold`, `MAX`, `IMAX`) with Admin authorization controls and test coverage.

## Context — Read These Files First
1. `Infrastructure/Database/AppDbContext.cs` — lines 1–40. EF Core context. Needs `DbSet<Hall> Halls` property and entity mapping in `OnModelCreating`.
2. `Extensions/EndpointRouteBuilderExtensions.cs` — lines 1–19. Central endpoint routing configuration. Needs `apiV1.MapHallEndpoints()`.
3. `Extensions/ServiceCollectionExtensions.cs` — lines 1–59. DI and Auth setup. Defines `AdminOnly` authorization policy (`RequireRole("Admin")`).
4. `Features/Auth/AuthEndpoints.cs` — lines 1–99. Reference precedent for endpoint group definitions and validation patterns.
5. `../users-auth/02-story-user-registration-and-authentication-2.md` — Precedent plan detailing authentication setup and test structure.

## Product rules (from story)
- **Current behaviour**: Application has no Cinema Hall data structures, endpoints, or hall capacity management.
- **New behaviour**:
  - `POST /api/v1/halls` (Admin only): Creates a new cinema hall with `Title`, `NumberOfSeats` (> 0), and `Type` (`Standard`, `4D`, `Gold`, `MAX`, `IMAX`). Sets `IsActive = true` and `CreatedAt = UTC`.
  - `GET /api/v1/halls`: Returns list of cinema halls, with optional query filtering (`?activeOnly=true`).
  - `GET /api/v1/halls/{id}`: Returns cinema hall details by ID.
  - `PUT /api/v1/halls/{id}` (Admin only): Updates hall details (`Title`, `NumberOfSeats`, `Type`) and updates `UpdatedAt = UTC`.
  - `PATCH /api/v1/halls/{id}/deactivate` (Admin only): Deactivates hall (`IsActive = false`) without physical hard-deletion.

## Implementation body

### 1 — Create HallType Enum
Create file: `Enums/HallType.cs`
Define string-convertible `HallType` enum:
```csharp
using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HallType
{
    Standard,
    [JsonPropertyName("4D")]
    FourD,
    Gold,
    MAX,
    IMAX
}
```

### 2 — Create Hall Domain Entity Model
Create file: `Models/Hall.cs`
Define `Hall` entity class:
```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Hall
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public HallType Type { get; set; } = HallType.Standard;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

### 3 — Create Hall DTOs
Create file: `DTOs/HallDtos.cs`
Define request and response record DTOs:
```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateHallRequest(string Title, int NumberOfSeats, HallType Type);

public record UpdateHallRequest(string Title, int NumberOfSeats, HallType Type);

public record HallDto(
    Guid Id,
    string Title,
    int NumberOfSeats,
    HallType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
```

### 4 — Update AppDbContext with Halls DbSet
File: `Infrastructure/Database/AppDbContext.cs`
Add `DbSet<Hall> Halls` and entity configuration in `OnModelCreating`:
```csharp
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Hall> Halls => Set<Hall>();

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NumberOfSeats).IsRequired();
            entity.Property(e => e.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
```

### 5 — Create Hall Endpoints
Create file: `Features/Halls/HallEndpoints.cs`
Implement CRUD and deactivation endpoints:
```csharp
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Halls;

public static class HallEndpoints
{
    public static void MapHallEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/halls").WithTags("Halls");

        // GET /api/v1/halls
        group.MapGet("", async (bool? activeOnly, AppDbContext db) =>
        {
            var query = db.Halls.AsQueryable();
            if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(h => h.IsActive);
            }

            var halls = await query
                .Select(h => new HallDto(h.Id, h.Title, h.NumberOfSeats, h.Type, h.IsActive, h.CreatedAt, h.UpdatedAt))
                .ToListAsync();

            return Results.Ok(halls);
        });

        // GET /api/v1/halls/{id}
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        });

        // POST /api/v1/halls (Admin Only)
        group.MapPost("", async (CreateHallRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Hall title is required." });
            }

            if (request.NumberOfSeats <= 0)
            {
                return Results.BadRequest(new { message = "Number of seats must be greater than zero." });
            }

            var hall = new Hall
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                NumberOfSeats = request.NumberOfSeats,
                Type = request.Type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Halls.Add(hall);
            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Created($"/api/v1/halls/{hall.Id}", hallDto);
        }).RequireAuthorization("AdminOnly");

        // PUT /api/v1/halls/{id} (Admin Only)
        group.MapPut("/{id:guid}", async (Guid id, UpdateHallRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Hall title is required." });
            }

            if (request.NumberOfSeats <= 0)
            {
                return Results.BadRequest(new { message = "Number of seats must be greater than zero." });
            }

            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            hall.Title = request.Title.Trim();
            hall.NumberOfSeats = request.NumberOfSeats;
            hall.Type = request.Type;
            hall.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        }).RequireAuthorization("AdminOnly");

        // PATCH /api/v1/halls/{id}/deactivate (Admin Only)
        group.MapPatch("/{id:guid}/deactivate", async (Guid id, AppDbContext db) =>
        {
            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            hall.IsActive = false;
            hall.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        }).RequireAuthorization("AdminOnly");
    }
}
```

### 6 — Map Hall Endpoints in EndpointRouteBuilderExtensions
File: `Extensions/EndpointRouteBuilderExtensions.cs`
Update `MapApplicationEndpoints`:
```csharp
using CinemaTicketsBack.Features.Auth;
using CinemaTicketsBack.Features.Halls;
using CinemaTicketsBack.Features.Movies;

namespace CinemaTicketsBack.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var apiV1 = app.MapGroup("/api/v1");

        apiV1.MapMovieEndpoints();
        apiV1.MapAuthEndpoints();
        apiV1.MapHallEndpoints();
        apiV1.MapHealthChecks("/health");

        return app;
    }
}
```

### 7 — Create EF Core Migration for Halls Table
Execute `dotnet ef migrations add AddHallsTable` in root directory to create the `Halls` table database schema migration.

---

## Edge Cases & Failure Modes
- **Zero or Negative Seats Count**: Triggered when `request.NumberOfSeats <= 0` during creation or update. Enforced in `HallEndpoints.cs`. Returns HTTP 400 Bad Request ("Number of seats must be greater than zero.").
- **Empty Hall Title**: Triggered when `request.Title` is null or whitespace. Enforced in `HallEndpoints.cs`. Returns HTTP 400 Bad Request ("Hall title is required.").
- **Invalid Hall Type JSON Deserialization**: Triggered when client passes unsupported hall type string (e.g. `"VIP"`). Enforced by System.Text.Json `JsonStringEnumConverter` returning HTTP 400 Bad Request.
- **Deactivated Hall soft delete**: Triggered when deactivating hall via `PATCH /api/v1/halls/{id}/deactivate`. Sets `IsActive = false` without executing EF Core `Remove()`. Historical records remain in DB intact.
- **Unauthorized / Non-Admin Access**: Triggered when an unauthenticated user or user without `Admin` role calls `POST`, `PUT`, or `PATCH`. Enforced by ASP.NET Core `RequireAuthorization("AdminOnly")` middleware returning HTTP 401 Unauthorized or HTTP 403 Forbidden.
- **Non-Existent Hall Request**: Triggered when querying, updating, or deactivating a non-existent `Guid`. Returns HTTP 404 Not Found ("Cinema hall not found.").

---

## Test Plan
1. **Integration Test - HallEndpointsTests**:
   File: `cinema-tickets-back.Tests/Endpoints/HallEndpointsTests.cs`
   - `CreateHall_AdminUser_Returns21Created`: Creates a new hall as Admin.
   - `CreateHall_ZeroSeats_Returns400BadRequest`: Rejects 0 or negative seats.
   - `GetHalls_ReturnsListOfHalls_WithActiveFilter`: Tests GET endpoint and `?activeOnly=true` parameter.
   - `UpdateHall_AdminUser_UpdatesHallDetails`: Updates title, capacity, and type.
   - `DeactivateHall_AdminUser_SetsIsActiveToFalse`: Soft deletes hall and verifies `IsActive == false`.
   - `CreateHall_NonAdminUser_Returns403Forbidden`: Verifies authorization enforcement.

---

## Migration / Rollback
- **Migration**: Run `dotnet ef migrations add AddHallsTable` to generate `Migrations/<timestamp>_AddHallsTable.cs`. Run `dotnet ef database update` to apply schema changes to SQL Server.
- **Rollback**: Run `dotnet ef database update AddUsersTable` to revert `Halls` table schema changes, then remove `AddHallsTable` migration files.

---

## Verification Steps
1. **Backend builds:** Run `dotnet build` in root directory `.`.
2. **EF Core migration creation:** Run `dotnet ef migrations add AddHallsTable` in root directory `.`.
3. **Test suite execution:** Run `dotnet test cinema-tickets-back.Tests/cinema-tickets-back.Tests.csproj` in root directory `.`.

---

## Done Criteria
- [ ] `POST /api/v1/halls` allows Admin users to create cinema halls with `Title`, `NumberOfSeats` (> 0), and `Type` (`Standard`, `4D`, `Gold`, `MAX`, `IMAX`).
- [ ] Zero or negative seat counts are rejected with HTTP 400 Bad Request.
- [ ] Unsupported hall types are rejected by API validation.
- [ ] `GET /api/v1/halls` returns cinema halls list with optional `?activeOnly=true` query filtering.
- [ ] `GET /api/v1/halls/{id}` returns specific hall details.
- [ ] `PUT /api/v1/halls/{id}` updates hall details (Admin only).
- [ ] `PATCH /api/v1/halls/{id}/deactivate` soft-deactivates a hall by setting `IsActive = false` (Admin only).
- [ ] Non-Admin users are blocked from management endpoints with HTTP 403 Forbidden.
- [ ] EF Core `AddHallsTable` migration created and applied.
- [ ] All unit and integration tests pass cleanly.
