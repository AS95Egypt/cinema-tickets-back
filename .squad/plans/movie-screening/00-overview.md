# movie-screening — plan overview

Entry point for the **movie-screening** feature. Stories execute in order by their `NN` prefix.

## Stories

| NN | File | Title | Tracker id | Depends on |
|----|------|-------|------------|------------|
| 05 | [05-story-user-story-5-movie-screening-management-5.md](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/movie-screening/05-story-user-story-5-movie-screening-management-5.md) | User Story 5 — Movie Screening Management | 5 | project-init-infra/01, users-auth/02, hall-management/03, movie-management/04 |

## Dependency notes

- **Depends on hall-management/03 + movie-management/04 jointly:** Screening has non-nullable FKs to both `Movie.Id` and `Hall.Id`; both parent entities must exist (and be `IsActive == true`) to create a screening. Story 03/04 patterns are reused for AdminOnly write endpoints, `IsActive` check, `DbUpdateException` handling, record DTO families, and EF Core entity configuration with `HasMaxLength` / `HasConversion` / `HasDefaultValue`.
- **Duration is derived from Movie.Duration (int minutes):** Story 04's `Movie.Duration` field is joined at query time (`screenings JOIN movies ON MovieId = movies.Id`) to compute screening end-time `StartDateTime + Duration minutes`. This is used both for customer-view grouping AND for the overlap detection query. The design intentionally does NOT store a screening-local Duration copy — as per the story requirement "should not be duplicated unless there is an explicit business requirement to preserve historical duration."
- **FK cascade is RESTRICT (no DELETE):** Screenings block hard-deletion of their parent Movie and Hall via `OnDelete(DeleteBehavior.Restrict)`. This complements the soft-delete patterns from Stories 03/04 — admins deactivate parents (PATCH deactivate) rather than hard-removing them, keeping historical screening rows intact for reporting. If a hard deletion is ever needed, it must be preceded by screening removal (done as a future cleanup story).
- **Unique index is a race guard, not the primary overlap check:** The DB has unique `(HallId, StartDateTime)` to catch concurrent exact-start collisions. The broader overlap check (half-open intervals `[start, end)`) is performed in the endpoint handler using a server-side query joining to Movies for duration.
- **Upstream consumers (Story 06+ Reservations):** The Reservation domain (next feature) will FK to `Screening.Id` and check `StartDateTime > DateTime.UtcNow` to enforce "past screenings cannot accept new reservations" business rule surfaced in this story's acceptance criteria.
- **Route convention is /api/v1, not bare /api:** Story 01 established `/api/v1` group in `EndpointRouteBuilderExtensions.cs:11`. Story 05 routes use `/api/v1/movies/{movieId}/screenings`, even though the GitHub story spec wrote bare `/api/movies/...`.
