# reservations-seats — plan overview

Entry point for the **reservations-seats** feature. Stories execute in order by their `NN` prefix.

## Stories

| NN | File | Title | Tracker id | Depends on |
|----|------|-------|------------|------------|
| 06 | [06-story-user-story-6-validate-seats-create-reservation-6.md](06-story-user-story-6-validate-seats-create-reservation-6.md) | User Story 6 — Validate Seats & Create Reservation | 6 | project-init-infra/01, users-auth/02, hall-management/03, movie-management/04, movie-screening/05 |
| 07 | `07-story-7.md` | User Story 7 — Reservation Availability | 7 | — |

## Dependency notes

- **Depends on Story 05 (movie-screening):** A reservation FKs to `Screening.Id` only. Hall capacity and price come from `Screening.Hall.NumberOfSeats` and `Screening.Price` after `.Include(s => s.Hall).Include(s => s.Movie)`. Do not trust client capacity or amount. Pattern for unique-index races is `try/catch (DbUpdateException)` in `Features/Screenings/ScreeningEndpoints.cs` (lines 105–112).
- **Enforces Story 05 “past screening cannot accept reservations”:** Create rejects `screening.StartDateTime <= DateTime.UtcNow`. Screening GET already filters future-by-default; that is not sufficient for POST.
- **Depends on Story 03 for `Hall.NumberOfSeats`:** Valid seats are `1..N` with no `HallSeats` table. Story 03 already requires `NumberOfSeats > 0` on hall write.
- **Depends on Story 02 for customer JWT:** `POST /api/v1/reservations` uses `.RequireAuthorization()` (any authenticated user, not `AdminOnly`). `UserId` comes from `JwtRegisteredClaimNames.Sub` / `ClaimTypes.NameIdentifier`, never from the body (`Services/JwtTokenGenerator.cs` line 31).
- **Status enum, not `IsApproved`:** Occupancy is `PENDING_PAYMENT` (unexpired) or `CONFIRMED`. Uniqueness is a **filtered** unique index on `(ScreeningId, SeatNo)` where `Status IN ('PENDING_PAYMENT', 'CONFIRMED')` so `CANCELLED` / `EXPIRED` can free the seat. Create also marks expired `PENDING_PAYMENT` rows `EXPIRED` in the same transaction so the index can accept a new hold.
- **FK cascade Restrict** on Reservation→User and Reservation→Screening, matching Screening→Movie/Hall in `AppDbContext.cs` lines 69–79.
- **Route prefix `/api/v1`:** Intake sample `POST /api/reservations` is implemented as `POST /api/v1/reservations` (`EndpointRouteBuilderExtensions.cs` line 12).
- **Out of this story (later work):** payment → `CONFIRMED`, customer cancel → `CANCELLED`, hosted expiry job, `GET` list/`/mine`, admin reservation APIs.
- **No HallSeats table:** Numeric `SeatNo` only.
