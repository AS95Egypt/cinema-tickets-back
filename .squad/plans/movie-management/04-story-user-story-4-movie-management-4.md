# Story 04 — User Story 4 — Movie Management (Story: 4)

## Prerequisites
- [Story 01 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/project-init-infra/01-story-initialize-cinema-reservation-api-1.md): ASP.NET Core 8.0 API project initialized with EF Core `AppDbContext` and SQL Server infrastructure.
- [Story 02 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/users-auth/02-story-user-registration-and-authentication-2.md): User authentication, JWT Bearer configuration, and `AdminOnly` authorization policy registered in DI pipeline.
- [Story 03 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/hall-management/03-story-cinema-hall-management-3.md): Pattern precedent for entity → DTO → endpoints with Admin authorization, soft-deactivation via `IsActive` flag, and EF Core migrations.

## Story Goal
Implement the full Movie Management feature for administrators: a `Movie` domain entity (with UUID/GUID Id, Title, Genre enum, Duration, ReleaseDate, Language, Description, Actors, TrailerUrl, IsActive, CreatedAt, UpdatedAt), a controlled `MovieGenre` enum (Comedy, Action, Drama, Fantasy — extendable), DTOs for request/response, complete EF Core mapping with proper constraints, and Minimal API endpoints for Create (`POST /api/v1/movies`), Get list with filtering (`GET /api/v1/movies`), Get by id (`GET /api/v1/movies/{id}`), Update (`PUT /api/v1/movies/{id}`), Deactivate (`PATCH /api/v1/movies/{id}/deactivate`), and a customer-facing Active Movies endpoint (`GET /api/v1/movies/active`) returning only `IsActive == true` movies with summary/detailed representations. Enforce business rules: required title, duration > 0, valid release date, URL validation for TrailerUrl. All management endpoints (POST/PUT/PATCH) require Admin authorization.

## Context — Read These Files First
1. `Models/Movie.cs` — lines 1–11. Current partial `Movie` entity with only 5 properties. Needs 7 new fields: `Language`, `Actors`, `TrailerUrl`, `IsActive`, `CreatedAt`, `UpdatedAt` + rename Genre from string to `MovieGenre` enum + rename `DurationMinutes` to `Duration` (minutes).
2. `Features/Movies/MovieEndpoints.cs` — lines 1–82. Current in-memory placeholder endpoints using a hard-coded `List<Movie>`. Needs full rewrite to use `AppDbContext`, DTOs, validation, Admin auth, and Active Movies endpoint.
3. `Infrastructure/Database/AppDbContext.cs` — lines 1–53. EF Core context. Has existing `DbSet<Movie>` (line 13) but incomplete `Movie` entity configuration in `OnModelCreating` (lines 21–28). Update to add all new property constraints: `Language`, `Actors`, `TrailerUrl`, `IsActive`, `CreatedAt`, `UpdatedAt`, and enum conversion for `Genre`.
4. `Features/Halls/HallEndpoints.cs` — lines 1–120. Direct pattern precedent: Admin-only `POST`/`PUT`/`PATCH`, public `GET`, `RequireAuthorization("AdminOnly")`, soft-deactivate via `IsActive = false`, `FindAsync` for lookups, DTO projections.
5. `Enums/HallType.cs` — lines 1–14. Precedent for enum with `JsonStringEnumConverter` and `JsonPropertyName` attribute. Follow same pattern for `MovieGenre`.
6. `DTOs/HallDtos.cs` — lines 1–17. Precedent for record DTO structure: CreateRequest, UpdateRequest, and detailed DTO with all fields including `IsActive`, `CreatedAt`, `UpdatedAt`.
7. `Extensions/EndpointRouteBuilderExtensions.cs` — lines 1–19. Central endpoint routing. `MapMovieEndpoints()` is already called at line 13 — no route registration changes needed, but confirm no `/api/movies` collision with story spec (codebase uses `/api/v1/movies`, spec says `/api/movies` — keep existing `/api/v1/` convention per project consistency).

---

## Product rules (from story)
- **Current behaviour**: Application has partial `Movie` model (5 fields), placeholder in-memory `MovieEndpoints` returning hard-coded list, no Admin auth on management endpoints, no genre control, no deactivation, no Active Movies endpoint.
- **New behaviour**:
  - `POST /api/v1/movies` (Admin only): Creates movie with validated required fields (Title, Genre, Duration > 0, valid ReleaseDate, optional TrailerUrl validated as URL if provided). Sets `IsActive = true`, `CreatedAt = UTC`.
  - `GET /api/v1/movies`: Returns all movies with optional query filters: `?genre=Action&activeOnly=true&title=Inception` (genre, active-only, title contains).
  - `GET /api/v1/movies/{id}`: Returns single movie by Guid.
  - `PUT /api/v1/movies/{id}` (Admin only): Updates mutable fields (Title, Genre, Duration, ReleaseDate, Language, Description, Actors, TrailerUrl), sets `UpdatedAt = UTC`.
  - `PATCH /api/v1/movies/{id}/deactivate` (Admin only): Soft-deactivates by setting `IsActive = false`, `UpdatedAt = UTC`. **No hard delete.**
  - `GET /api/v1/movies/active`: **Public endpoint.** Returns only movies where `IsActive == true`. Supports `?view=summary|detailed` query param: summary omits `Description`, `Actors`, `TrailerUrl`, `CreatedAt`, `UpdatedAt`; detailed includes everything.

---

## Implementation Tasks

### 1 — Create MovieGenre Enum
Create file: `Enums/MovieGenre.cs`

Follow the `HallType.cs` pattern (JSON string enum converter). Define the 4 genres specified, as strings.

```csharp
using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovieGenre
{
    Comedy,
    Action,
    Drama,
    Fantasy
}
```

**Note:** The list can be extended later by adding enum members. No `JsonPropertyName` needed for these — names are clean.

---

### 2 — Update Movie Domain Entity Model
File: `Models/Movie.cs`

**Replace** the entire existing class (lines 1–11). The new `Movie` entity must include:
- Swap `Genre` from `string` to `MovieGenre` enum.
- Rename `DurationMinutes` to `Duration` (minutes stored as int, per story).
- Add `Language` (string, nullable? No — required per story data).
- Add `Actors` (string, nullable — optional field).
- Add `TrailerUrl` (string, nullable — optional field).
- Add `IsActive` (bool, default true).
- Add `CreatedAt` (DateTime, default UTC now).
- Add `UpdatedAt` (DateTime?, nullable).

```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public MovieGenre Genre { get; set; } = MovieGenre.Action;
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Actors { get; set; }
    public string? TrailerUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

**Breaking note:** `DurationMinutes` → `Duration` and `Genre` string → enum. Since `MovieEndpoints` are being rewritten (step 5) and no other code references these fields, this is safe.

---

### 3 — Create Movie DTOs
Create file: `DTOs/MovieDtos.cs`

Mirror `HallDtos.cs` record pattern. Add 2 request records, a detailed DTO, and a summary DTO for the Active Movies endpoint.

```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateMovieRequest(
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl
);

public record UpdateMovieRequest(
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl
);

public record MovieDto(
    Guid Id,
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MovieSummaryDto(
    Guid Id,
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language
);
```

---

### 4 — Update AppDbContext Movie Entity Configuration
File: `Infrastructure/Database/AppDbContext.cs`

**Update lines 21–28** inside `modelBuilder.Entity<Movie>(entity => { ... })` block. Keep existing `HasKey`, `Title`, `Description` constraints. Update the following:
- Rename `DurationMinutes` → `Duration` in property config.
- Change `Genre` config: use `.HasConversion<string>()` (same pattern as `Hall.Type` at lines 46–49), `HasMaxLength(50)`, `IsRequired()`.
- Add `Language`: `IsRequired().HasMaxLength(100)`.
- Add `Actors`: `HasMaxLength(500)` (optional, no `IsRequired`).
- Add `TrailerUrl`: `HasMaxLength(1000)` (optional).
- Add `IsActive`: `HasDefaultValue(true)`.
- Add `CreatedAt`: no default needed (model default), but confirm EF Core maps correctly.
- Add `UpdatedAt`: nullable DateTime, no default needed.

```csharp
modelBuilder.Entity<Movie>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Genre)
        .HasConversion<string>()
        .IsRequired()
        .HasMaxLength(50);
    entity.Property(e => e.Duration).IsRequired();
    entity.Property(e => e.ReleaseDate).IsRequired();
    entity.Property(e => e.Language).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Description).HasMaxLength(1000);
    entity.Property(e => e.Actors).HasMaxLength(500);
    entity.Property(e => e.TrailerUrl).HasMaxLength(1000);
    entity.Property(e => e.IsActive).HasDefaultValue(true);
});
```

The `DbSet<Movie>` property on line 13 is already correct; leave it unchanged.

---

### 5 — Rewrite MovieEndpoints with Full CRUD + Active Endpoint
File: `Features/Movies/MovieEndpoints.cs`

**Replace the entire file content** (lines 1–82). Follow `HallEndpoints.cs` pattern plus:
- Admin-only on POST, PUT, PATCH.
- Public GET and Active endpoint.
- URL validation helper for `TrailerUrl` (use `Uri.TryCreate`).
- Filtering on list endpoint (`genre`, `activeOnly`, `title` contains).
- `GET /api/v1/movies/active` with `?view=summary|detailed` param (default: `detailed`). Route order: register `/active` BEFORE `/{id:guid}` to avoid route collision (Minimal APIs use order-first-match; putting `/active` higher solves it).
- `Summary` vs `Detailed` projection: use `MovieSummaryDto` and `MovieDto` respectively.
- **No DELETE endpoint.** Business rule #6 prohibits hard deletion. Remove the `MapDelete` handler from current file.

```csharp
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Movies;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies").WithTags("Movies");

        // GET /api/v1/movies/active — Public, customer-facing. Register BEFORE /{id} to avoid route conflict.
        group.MapGet("/active", async (string? view, AppDbContext db) =>
        {
            var isSummary = string.Equals(view, "summary", StringComparison.OrdinalIgnoreCase);
            var query = db.Movies.Where(m => m.IsActive);

            if (isSummary)
            {
                var summaries = await query
                    .Select(m => new MovieSummaryDto(
                        m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate, m.Language))
                    .ToListAsync();
                return Results.Ok(summaries);
            }

            var detailed = await query
                .Select(m => new MovieDto(
                    m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate,
                    m.Language, m.Description, m.Actors, m.TrailerUrl,
                    m.IsActive, m.CreatedAt, m.UpdatedAt))
                .ToListAsync();
            return Results.Ok(detailed);
        });

        // GET /api/v1/movies — Admin/all listing with optional filtering
        group.MapGet("", async (MovieGenre? genre, bool? activeOnly, string? title, AppDbContext db) =>
        {
            var query = db.Movies.AsQueryable();

            if (genre.HasValue)
            {
                query = query.Where(m => m.Genre == genre.Value);
            }
            if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(m => m.IsActive);
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.Contains(title.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            var movies = await query
                .Select(m => new MovieDto(
                    m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate,
                    m.Language, m.Description, m.Actors, m.TrailerUrl,
                    m.IsActive, m.CreatedAt, m.UpdatedAt))
                .ToListAsync();

            return Results.Ok(movies);
        });

        // GET /api/v1/movies/{id}
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        });

        // POST /api/v1/movies (Admin Only)
        group.MapPost("", async (CreateMovieRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Movie title is required." });
            }
            if (request.Duration <= 0)
            {
                return Results.BadRequest(new { message = "Duration must be greater than zero." });
            }
            if (request.ReleaseDate == default)
            {
                return Results.BadRequest(new { message = "Valid release date is required." });
            }
            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return Results.BadRequest(new { message = "Language is required." });
            }
            if (!string.IsNullOrWhiteSpace(request.TrailerUrl) &&
                !Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
            {
                return Results.BadRequest(new { message = "Trailer URL must be a valid absolute URL when provided." });
            }

            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Genre = request.Genre,
                Duration = request.Duration,
                ReleaseDate = request.ReleaseDate,
                Language = request.Language.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Actors = string.IsNullOrWhiteSpace(request.Actors) ? null : request.Actors.Trim(),
                TrailerUrl = string.IsNullOrWhiteSpace(request.TrailerUrl) ? null : request.TrailerUrl.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Created($"/api/v1/movies/{movie.Id}", dto);
        }).RequireAuthorization("AdminOnly");

        // PUT /api/v1/movies/{id} (Admin Only)
        group.MapPut("/{id:guid}", async (Guid id, UpdateMovieRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Movie title is required." });
            }
            if (request.Duration <= 0)
            {
                return Results.BadRequest(new { message = "Duration must be greater than zero." });
            }
            if (request.ReleaseDate == default)
            {
                return Results.BadRequest(new { message = "Valid release date is required." });
            }
            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return Results.BadRequest(new { message = "Language is required." });
            }
            if (!string.IsNullOrWhiteSpace(request.TrailerUrl) &&
                !Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
            {
                return Results.BadRequest(new { message = "Trailer URL must be a valid absolute URL when provided." });
            }

            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movie.Title = request.Title.Trim();
            movie.Genre = request.Genre;
            movie.Duration = request.Duration;
            movie.ReleaseDate = request.ReleaseDate;
            movie.Language = request.Language.Trim();
            movie.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            movie.Actors = string.IsNullOrWhiteSpace(request.Actors) ? null : request.Actors.Trim();
            movie.TrailerUrl = string.IsNullOrWhiteSpace(request.TrailerUrl) ? null : request.TrailerUrl.Trim();
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        }).RequireAuthorization("AdminOnly");

        // PATCH /api/v1/movies/{id}/deactivate (Admin Only) — soft delete, no hard remove
        group.MapPatch("/{id:guid}/deactivate", async (Guid id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movie.IsActive = false;
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        }).RequireAuthorization("AdminOnly");
    }
}
```

---

### 6 — Create EF Core Migration for Updated Movies Schema
Execute in root directory `.`:

```
dotnet ef migrations add UpdateMoviesTableWithFullSchema
```

This generates `Migrations/<timestamp>_UpdateMoviesTableWithFullSchema.cs` and `*.Designer.cs`, and updates `AppDbContextModelSnapshot.cs`. The migration should:
- Rename column `DurationMinutes` → `Duration` (or drop + add; use explicit RenameColumn in migration if EF generates drop+add to preserve data, though no production data exists yet).
- Alter `Genre` from `nvarchar(100)` → `nvarchar(50)` + constraint (already string-stored; EF conversion handles it).
- Add columns: `Language nvarchar(100) NOT NULL`, `Actors nvarchar(500) NULL`, `TrailerUrl nvarchar(1000) NULL`, `IsActive bit NOT NULL default 1`, `CreatedAt datetime2 NOT NULL`, `UpdatedAt datetime2 NULL`.

Then run:
```
dotnet ef database update
```

---

### 7 — Confirm EndpointRouteBuilderExtensions Needs No Changes
File: `Extensions/EndpointRouteBuilderExtensions.cs`

Line 13 already calls `apiV1.MapMovieEndpoints();` — no edit required. Verify the call still compiles after `MovieEndpoints.cs` rewrite (it will — signature `MapMovieEndpoints(this IEndpointRouteBuilder app)` is unchanged).

---

## Edge Cases & Failure Modes
- **Empty/Whitespace Movie Title**: Triggered when `request.Title` is null or whitespace. Enforced in `MovieEndpoints.cs` POST/PUT validators. Returns HTTP 400 ("Movie title is required.").
- **Zero or Negative Duration**: Triggered when `request.Duration <= 0`. Enforced in POST/PUT. Returns HTTP 400 ("Duration must be greater than zero.").
- **Default/Invalid Release Date**: Triggered when `request.ReleaseDate == default(DateTime)` (0001-01-01). Enforced in POST/PUT. Returns HTTP 400 ("Valid release date is required.").
- **Missing Language**: Triggered when `request.Language` is null/whitespace. Enforced in POST/PUT. Returns HTTP 400 ("Language is required.").
- **Invalid TrailerUrl Format**: Triggered when `TrailerUrl` is non-empty and `Uri.TryCreate(..., UriKind.Absolute, out _)` returns false. Enforced in POST/PUT. Returns HTTP 400 ("Trailer URL must be a valid absolute URL when provided.").
- **Unsupported Genre String**: Triggered when client passes JSON genre value not in `MovieGenre` enum (e.g. `"Horror"`). Enforced by System.Text.Json `JsonStringEnumConverter` on `MovieGenre`. Returns HTTP 400 Bad Request automatically.
- **Inactive Movies Hidden From /active Endpoint**: Triggered when `IsActive == false` movies exist. Enforced in `GET /api/v1/movies/active` via `Where(m => m.IsActive)` query filter. Verifies business rule #5.
- **No Hard Delete Endpoint**: The old `MapDelete` handler is intentionally removed. Attempting `DELETE /api/v1/movies/{id}` returns HTTP 404. Enforces business rules #6 & #7 (preserves historical screenings + reservations).
- **Route Order Conflict**: `GET /movies/active` must be registered BEFORE `GET /movies/{id:guid}`; otherwise `active` would be parsed as a GUID (fails route constraint) and fall through. Enforced by code order in `MovieEndpoints.cs`.
- **Unauthorized Admin Access**: Triggered when unauthenticated user or non-Admin calls POST/PUT/PATCH. Enforced by `RequireAuthorization("AdminOnly")`. Returns HTTP 401 (unauthenticated) or HTTP 403 (non-admin role).
- **Non-Existent Movie Guid**: Triggered on GET/PUT/PATCH for unknown id. `FindAsync` returns null. Returns HTTP 404 ("Movie not found.").
- **SQL Server Concurrency**: Multiple Admin users updating the same movie simultaneously. EF Core uses last-write-wins by default (no concurrency token defined). Acceptable for this story; escalate to `[Timestamp]` if conflicts occur in future.

---

## Test Plan
Follow the testing pattern established in the hall-management story (tests in separate test project if it exists; if not, create unit tests in a new xUnit test project `cinema-tickets-back.Tests`).

1. **Unit Test — URL Validation Helper Logic** *(extract `IsValidUrl` if tested, otherwise inline in integration)*:
   - File: `cinema-tickets-back.Tests/Features/Movies/MovieEndpointValidationTests.cs`
   - `TrailerUrl_ValidAbsoluteUrl_Passes`: `"https://example.com/trailer"` → valid.
   - `TrailerUrl_RelativeUrl_Fails`: `"/trailer"` → invalid.
   - `TrailerUrl_Malformed_Fails`: `"not a url"` → invalid.
   - `TrailerUrl_NullEmpty_IsOptional`: `null` / `""` → accepted.

2. **Integration Test — Movie Creation (Admin)**
   - File: `cinema-tickets-back.Tests/Features/Movies/MovieEndpointsTests.cs`
   - `CreateMovie_ValidData_Returns201Created`: Admin calls POST with valid payload. Asserts 201, Location header, `IsActive == true`, `CreatedAt` recent.
   - `CreateMovie_EmptyTitle_Returns400BadRequest`: `Title = ""` → 400.
   - `CreateMovie_DurationZero_Returns400BadRequest`: `Duration = 0` → 400.
   - `CreateMovie_DurationNegative_Returns400BadRequest`: `Duration = -10` → 400.
   - `CreateMovie_InvalidTrailerUrl_Returns400BadRequest`: `TrailerUrl = "ftp://bad"` or `"foo"` → 400.
   - `CreateMovie_NonAdminUser_Returns403Forbidden`: JWT without Admin role → 403.
   - `CreateMovie_Unauthenticated_Returns401Unauthorized`: No JWT → 401.

3. **Integration Test — Movie Query Endpoints**
   - `GetMovies_ReturnsList_WithGenreFilter`: Seed 3 movies, query `?genre=Action` → only Action movies.
   - `GetMovies_ActiveOnlyFilter_ExcludesInactive`: Seed 2 active + 1 deactivated, `?activeOnly=true` → count = 2.
   - `GetMovies_TitleFilter_CaseInsensitiveContains`: Title "Inception 2", query `?title=inception` → matches.
   - `GetMovieById_ExistingId_Returns200WithMovieDto`.
   - `GetMovieById_NonExistentId_Returns404NotFound`.

4. **Integration Test — Movie Update (Admin)**
   - `UpdateMovie_ValidData_Returns200_UpdatesFields`: PUT to existing id; verifies Title, Genre, Duration, Language, Description changed, `UpdatedAt` set, `CreatedAt` unchanged.
   - `UpdateMovie_InvalidDuration_Returns400`: Same validation as POST.
   - `UpdateMovie_NonExistentId_Returns404`.
   - `UpdateMovie_NonAdmin_Returns403`.

5. **Integration Test — Movie Deactivation (Admin)**
   - `DeactivateMovie_ExistingId_SetsIsActiveFalse_Returns200`: PATCH to active movie; asserts `IsActive == false`, `UpdatedAt` set.
   - `DeactivateMovie_NonExistentId_Returns404`.
   - `DeactivateMovie_NonAdmin_Returns403`.
   - `DeactivateMovie_ThenQueryActive_Excluded`: After deactivate, `GET /movies/active` does NOT include it. Verifies business rule #5.

6. **Integration Test — Active Movies Endpoint (Public)**
   - `GetActiveMovies_OnlyReturnsIsActiveTrue`: Seed mix, assert count.
   - `GetActiveMovies_SummaryView_OmitsSensitiveDetailFields`: `?view=summary` → response JSON lacks `Description`, `Actors`, `TrailerUrl`, `CreatedAt`, `UpdatedAt`.
   - `GetActiveMovies_DetailedView_IncludesAllFields`: `?view=detailed` (or default) → all fields present.

7. **Integration Test — No Hard Delete Endpoint**
   - `DeleteMovie_Returns404NotFound`: Ensures `DELETE /api/v1/movies/{id}` has no route → 404. Confirms business rules #6/#7.

---

## Migration / Rollback
- **Migration forward**:
  1. `dotnet ef migrations add UpdateMoviesTableWithFullSchema`
  2. `dotnet ef database update`
- **Half-applied state risk**: Migration adds 6 columns + renames 1. If the rename step (`DurationMinutes` → `Duration`) fails halfway (e.g., timeout), the DB may have new columns but old `DurationMinutes`. EF Core migration is transactional by default on SQL Server — the entire migration rolls back on error.
- **Rollback**:
  1. `dotnet ef database update AddHallsTable` — reverts DB schema to pre-movie migration (removes the new `Movie` columns and any Genre column alteration).
  2. Delete the `Migrations/<timestamp>_UpdateMoviesTableWithFullSchema.*` files (both `.cs` and `.Designer.cs`).
  3. Revert `AppDbContextModelSnapshot.cs` to pre-migration state (or let next `migrations add` regenerate it).

---

## Verification Steps
1. **Backend builds:** Run `dotnet build` in root directory `.`. Confirm 0 errors, 0 warnings about enum conversion or missing `Movie` props.
2. **EF Core migration:** Run `dotnet ef migrations add UpdateMoviesTableWithFullSchema` in `.`. Confirm file created under `Migrations/` with expected column additions.
3. **Apply migration locally:** Run `dotnet ef database update` in `.`. Confirm tables updated via SQL Server Object Explorer or `dotnet ef dbcontext info`.
4. **Smoke-test endpoints:**
   - `dotnet run` (or `dotnet watch run`) in `.`.
   - `GET https://localhost:<port>/api/v1/movies/active?view=summary` → 200, empty array `[]` (no data yet).
   - Login as Admin via `POST /api/v1/auth/login` → obtain JWT.
   - `POST /api/v1/movies` with JWT + sample payload from story → 201 Created.
   - `PATCH /api/v1/movies/<id>/deactivate` with JWT → 200, `IsActive: false`.
   - `GET /api/v1/movies/active` → empty (deactivated movie excluded).
5. **Test suite:** Run `dotnet test cinema-tickets-back.Tests/cinema-tickets-back.Tests.csproj` in `.`. All tests green.

---

## Done Criteria
- [ ] `Enums/MovieGenre.cs` created with `Comedy`, `Action`, `Drama`, `Fantasy` and `JsonStringEnumConverter`.
- [ ] `Models/Movie.cs` updated to full 12-property entity (Id, Title, Genre enum, Duration, ReleaseDate, Language, Description, Actors, TrailerUrl, IsActive, CreatedAt, UpdatedAt).
- [ ] `DTOs/MovieDtos.cs` created with `CreateMovieRequest`, `UpdateMovieRequest`, `MovieDto`, `MovieSummaryDto` records.
- [ ] `Infrastructure/Database/AppDbContext.cs` `Movie` entity configuration updated: Genre stored as string via `HasConversion<string>`, `Language` required max 100, `Actors` max 500, `TrailerUrl` max 1000, `IsActive` default true.
- [ ] `Features/Movies/MovieEndpoints.cs` fully rewritten:
  - [ ] `GET /api/v1/movies/active?view=summary|detailed` registered BEFORE `/{id}`, returns only `IsActive == true` movies.
  - [ ] `GET /api/v1/movies` supports `?genre=`, `?activeOnly=`, `?title=` filters.
  - [ ] `GET /api/v1/movies/{id}` returns detailed DTO or 404.
  - [ ] `POST /api/v1/movies` (Admin only): Validates title required, duration > 0, releaseDate valid, language required, TrailerUrl URL format if provided. Sets `IsActive = true`, `CreatedAt = UTC`.
  - [ ] `PUT /api/v1/movies/{id}` (Admin only): Same validations, sets `UpdatedAt = UTC`.
  - [ ] `PATCH /api/v1/movies/{id}/deactivate` (Admin only): Sets `IsActive = false`, `UpdatedAt = UTC`.
  - [ ] No `DELETE /api/v1/movies/{id}` endpoint exists.
- [ ] EF Core `UpdateMoviesTableWithFullSchema` migration generated and applied cleanly.
- [ ] `dotnet build` succeeds with 0 errors.
- [ ] `GET /api/v1/movies/active` excludes deactivated movies (AC #6).
- [ ] Duration ≤ 0 is rejected with HTTP 400 (AC #5).
- [ ] All acceptance criteria from story: Admin can create / update / view / deactivate; active listing works; historical data preserved.
- [ ] All unit & integration tests pass.
