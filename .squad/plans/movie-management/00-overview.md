# movie-management — plan overview

Entry point for the **movie-management** feature. Stories execute in order by their `NN` prefix.

## Stories

| NN | File | Title | Tracker id | Depends on |
|----|------|-------|------------|------------|
| 04 | [04-story-user-story-4-movie-management-4.md](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/movie-management/04-story-user-story-4-movie-management-4.md) | User Story 4 — Movie Management | 4 | project-init-infra/01, users-auth/02, hall-management/03 |

## Dependency notes

- **Depends on hall-management/03 pattern precedent:** Uses identical AdminOnly authorization, soft-deactivation via `IsActive` flag, EF Core entity configuration pattern, and DTO record structure defined in Story 03.
- **Upstream shared contracts:** `AppDbContext.Movies` DbSet already exists from Story 01; Story 04 replaces the partial entity definition in place.
- **Downstream consumers:** Future screening-scheduling stories will reference `Movie.Id`, `Movie.IsActive`, and `Movie.Duration`; deactivation (not deletion) ensures FK integrity with historical screenings and reservations per business rules #6 and #7.
