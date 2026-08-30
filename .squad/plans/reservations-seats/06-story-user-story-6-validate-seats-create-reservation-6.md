# Story 06 — User Story 6 — Validate Seats & Create Reservation (Story: 6)

## Prerequisites
- [Story 01 completed](../project-init-infra/01-story-initialize-cinema-reservation-api-1.md): ASP.NET Core 8.0 API, EF Core `AppDbContext`, SQL Server, `/api/v1` route group.
- [Story 02 completed](../users-auth/02-story-user-registration-and-authentication-2.md): JWT Bearer auth, `User` entity, login issues a token with `JwtRegisteredClaimNames.Sub` = `user.Id`.
- [Story 03 completed](../hall-management/03-story-cinema-hall-management-3.md): `Hall.NumberOfSeats` is the authoritative 1..N seat range; POST/PUT already reject `NumberOfSeats <= 0`.
- [Story 04 completed](../movie-management/04-story-user-story-4-movie-management-4.md): `Movie.IsActive` exists and is used as the soft-active flag.
- [Story 05 completed](../movie-screening/05-story-user-story-5-movie-screening-management-5.md): `Screening` entity with `HallId`, `MovieId`, `StartDateTime`, `Price`; FKs `OnDelete(DeleteBehavior.Restrict)`; unique `(HallId, StartDateTime)`; POST wraps `SaveChangesAsync` in `try/catch (DbUpdateException)` — **copy that race-guard pattern** for reservations.

## Story Goal
Customers authenticated with JWT can `POST /api/v1/reservations` with `{ screeningId, seatNo }` to place a **temporary hold** on a numeric seat for a **future** screening. The server loads the screening (and its hall + movie) from SQL Server, validates the seat against that hall's `NumberOfSeats`, rejects occupied seats, and inserts a reservation in `PENDING_PAYMENT` with `ExpiresAt = CreatedAt + configurable hold duration` and `Amount = Screening.Price`. Concurrent requests for the same `(ScreeningId, SeatNo)` must not both succeed; the database unique constraint is the last line of defence, not a check-then-insert race.

**Not in scope:** Payment capture / webhook / mark-as-`CONFIRMED`; cancel/expire background job as a hosted service; listing reservations (`GET /mine`); admin reservation APIs; a `HallSeats` table; client-supplied amount, currency, capacity, or user id.

---

## Context — Read These Files First
1. `Models/Hall.cs` — lines 5–14. Seat capacity is `NumberOfSeats` (line 9). Valid seats are **1 through `NumberOfSeats` inclusive**. There is no seat-row table.
2. `Models/Screening.cs` — lines 3–15. Reservation **must FK to `Screening.Id`**, not to movie/hall alone. Use navigation `Hall` and `Movie` (lines 13–14) with `.Include(...)` the same way `Features/Screenings/ScreeningEndpoints.cs` does at lines 30–31. Price for the hold is `Screening.Price` (line 9). Future check uses `StartDateTime` (line 8).
3. `Models/Movie.cs` — lines 5–18. Gate create on `IsActive` (line 16) of the screening's movie.
4. `Models/User.cs` — lines 3–12. Reservation `UserId` FK → `User.Id` (line 5). Do **not** accept `userId` from the JSON body.
5. `Infrastructure/Database/AppDbContext.cs` — lines 13–82. Add `DbSet<Reservation>` after `Screenings` (line 16). Add a `modelBuilder.Entity<Reservation>(...)` block **after** the Screening block (ends line 82), before the closing brace of `OnModelCreating` (line 83). Mirror Screening FKs: `OnDelete(DeleteBehavior.Restrict)` (lines 69–79) and a uniqueness index (line 81 pattern, but **filtered** — see Task 5).
6. `Features/Screenings/ScreeningEndpoints.cs` — lines 44–123. Handler style: sequential `Find`/`Include` lookups, `Results.BadRequest(new { message = "..." })`, `Results.NotFound`, `Created` URL under `/api/v1/...`, `try/catch (DbUpdateException)` at lines 105–112. Inactive movie/hall messages at lines 51–63. Past-start check at lines 66–68 (`StartDateTime <= DateTime.UtcNow.AddMinutes(-1)` is for **creating** a screening; reservation create must reject a screening that **has already started**: `StartDateTime <= DateTime.UtcNow`).
7. `Features/Halls/HallEndpoints.cs` — lines 8–71. Group + `.WithTags(...)`, `.RequireAuthorization(...)` on writes, `Results.Created($"/api/v1/...", dto)`. Capacity validation precedent: `request.NumberOfSeats <= 0` at lines 51–54.
8. `Features/Auth/AuthEndpoints.cs` — lines 59–82. Login returns JWT. Inactive users already blocked at login (lines 74–77); still bind reservation `UserId` from claims, not the body.
9. `Services/JwtTokenGenerator.cs` — lines 29–36. Subject claim is `new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())` (line 31). Role is `ClaimTypes.Role` `"Admin"` / `"User"` (line 34). Extract user id from `ClaimsPrincipal` using `ClaimTypes.NameIdentifier` **and** `JwtRegisteredClaimNames.Sub` / `"sub"` (JWT inbound mapping may rewrite `sub`).
10. `Extensions/EndpointRouteBuilderExtensions.cs` — lines 10–18. Group is `MapGroup("/api/v1")` (line 12). Register `apiV1.MapReservationEndpoints();` after `MapScreeningEndpoints()` (line 17). Intake sample path `POST /api/reservations` **must** become `POST /api/v1/reservations` (same convention as Story 05).
11. `Extensions/ServiceCollectionExtensions.cs` — lines 12–56. `AddAuthorization` + `AdminOnly` at lines 48–51. Reservation POST is **any authenticated user**: `.RequireAuthorization()` with **no** `"AdminOnly"` policy. Register `IOptions<ReservationSettings>` (or bind `ReservationSettings` from config) next to the existing scoped services (lines 20–22).
12. `appsettings.json` — lines 1–18. Add a `ReservationSettings` section sibling to `JwtSettings` (starts line 12). Do **not** hard-code hold duration.
13. `Enums/HallType.cs` — lines 1–14. Copy `[JsonConverter(typeof(JsonStringEnumConverter))]` for `ReservationStatus`.
14. `Program.cs` — lines 24–29. `UseAuthentication` / `UseAuthorization` already run before mapped endpoints. No Program.cs change required unless you add options binding there instead of `AddApplicationServices`.
15. `cinema-tickets-back.csproj` — lines 1–27. `net8.0`, nullable enabled. Test project is **not** in this csproj (`DefaultItemExcludes` excludes `cinema-tickets-back.Tests\**` at line 8). If tests are added, scaffold a sibling test project as Story 05's test plan describes.
16. Intake (no attachments): `.squad/stories/reservations-seats/6/intake.md`.
17. Precedent plan: `../movie-screening/05-story-user-story-5-movie-screening-management-5.md` — entity + DTO records + `AppDbContext` + Minimal API feature folder + EF migration + unique-index race catch.

- Grep for `MapScreeningEndpoints` in `Extensions/EndpointRouteBuilderExtensions.cs` before adding `MapReservationEndpoints`.
- Grep for `Reservation` / `SeatNo` in `*.cs` — they must **not** exist yet; do not invent a `HallSeat` model.

---

## Product rules (from story)
- **Current behaviour:** No reservation table, no seat hold, no `POST` reservations. Screening GET already hides past screenings by default (`ScreeningEndpoints.cs` lines 25–28); that does **not** enforce reservation create — this story does.
- **New behaviour:**
  - Authenticated `POST /api/v1/reservations` with body `{ "screeningId": "<guid>", "seatNo": <int> }` only.
  - Server loads screening + hall + movie from DB. Client **cannot** send hall capacity, price, currency, user id, or status.
  - Valid `seatNo` is `1..Hall.NumberOfSeats`. `0`, negatives, and `> NumberOfSeats` → 400.
  - Screening missing → 404. Inactive movie or hall → 400. Screening already started (`StartDateTime <= DateTime.UtcNow`) → 400.
  - Active occupancy = status `CONFIRMED`, or `PENDING_PAYMENT` with `ExpiresAt > UtcNow`. Same numeric seat **may** be reserved on a **different** screening.
  - Insert status **`PENDING_PAYMENT`**, `Amount = screening.Price`, `Currency` from config (default `"EGP"`), `ExpiresAt = CreatedAt + HoldDuration`. Status enum, **not** `IsApproved`.
  - Uniqueness of active holds is enforced in SQL (filtered unique index) **and** by catching `DbUpdateException` like screening create.
  - Payment success is **not** implemented; reservations stay unconfirmed (`PENDING_PAYMENT`). Do not add a confirm endpoint in this story.

---

## Implementation tasks

### 1 — Create `ReservationStatus` enum
Create file: `Enums/ReservationStatus.cs`

Match `Enums/HallType.cs` JSON converter so the API returns `"PENDING_PAYMENT"` not `0`.

```csharp
using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationStatus
{
    PENDING_PAYMENT,
    CONFIRMED,
    CANCELLED,
    EXPIRED
}
```

This story **only writes** `PENDING_PAYMENT` (and may set `EXPIRED` on **stale holds for the same seat** inside the create transaction so the unique index can release the seat). Do **not** set `CONFIRMED` or `CANCELLED` on the create path.

---

### 2 — Create `Reservation` entity
Create file: `Models/Reservation.cs`

Follow `Models/Screening.cs` (Guid PK, UTC timestamps, navigation properties). **No `IsApproved`.** **No `HallSeats`.** Seat is `int SeatNo`.

```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ScreeningId { get; set; }
    public int SeatNo { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.PENDING_PAYMENT;
    public DateTime ExpiresAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Screening Screening { get; set; } = null!;
}
```

---

### 3 — Create reservation DTOs
Create file: `DTOs/ReservationDtos.cs`

Follow `DTOs/ScreeningDtos.cs` record style. Request has **only** the two intake fields. Response matches the intake JSON (camelCase via default serializer). **Do not** put `Amount` or `Currency` on the request.

```csharp
using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateReservationRequest(Guid ScreeningId, int SeatNo);

public record ReservationDto(
    Guid ReservationId,
    Guid ScreeningId,
    int SeatNo,
    ReservationStatus Status,
    DateTime ExpiresAt,
    decimal Amount,
    string Currency
);
```

Map `Reservation.Id` → `ReservationId` in the handler (intake name `reservationId`).

---

### 4 — Add `ReservationSettings` and bind config
Create file: `Services/ReservationSettings.cs` (or `Configuration/ReservationSettings.cs` if you prefer a config folder — **use `Services/`** to stay next to existing helpers).

```csharp
namespace CinemaTicketsBack.Services;

public class ReservationSettings
{
    public const string SectionName = "ReservationSettings";
    public int HoldDurationMinutes { get; set; } = 15;
    public string Currency { get; set; } = "EGP";
}
```

File: `appsettings.json` — add after `JwtSettings` (after line 17):

```json
  "ReservationSettings": {
    "HoldDurationMinutes": 15,
    "Currency": "EGP"
  }
```

File: `Extensions/ServiceCollectionExtensions.cs` — inside `AddApplicationServices`, after line 22:

```csharp
services.Configure<ReservationSettings>(
    configuration.GetSection(ReservationSettings.SectionName));
```

Add `using` if the class lives in `CinemaTicketsBack.Services`. Inject `IOptions<ReservationSettings>` into the POST handler. If `HoldDurationMinutes <= 0`, treat as invalid config: throw `InvalidOperationException` at the start of the handler (fails closed; do not silently use 0-minute holds).

---

### 5 — Update `AppDbContext`
File: `Infrastructure/Database/AppDbContext.cs`

**5a)** After line 16:

```csharp
public DbSet<Reservation> Reservations => Set<Reservation>();
```

**5b)** After the Screening `entity` block (after line 82), add Reservation mapping:

- `HasKey(e => e.Id)`
- `SeatNo` required
- `Status`: `HasConversion<string>()`, `IsRequired()`, `HasMaxLength(32)` (same conversion style as `Hall.Type` at lines 55–58)
- `Amount`: `HasPrecision(18, 2).IsRequired()` (copy Screening `Price` at line 67)
- `Currency`: `IsRequired()`, `HasMaxLength(8)`
- `ExpiresAt` required
- `HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict).IsRequired()`
- `HasOne(r => r.Screening).WithMany().HasForeignKey(r => r.ScreeningId).OnDelete(DeleteBehavior.Restrict).IsRequired()`
- **Filtered unique index** — this is the concurrency contract:

```csharp
entity.HasIndex(e => new { e.ScreeningId, e.SeatNo })
    .IsUnique()
    .HasFilter("[Status] IN (N'PENDING_PAYMENT', N'CONFIRMED')")
    .HasDatabaseName("IX_Reservations_ScreeningId_SeatNo_Active");
```

**Why filtered:** `CANCELLED` and `EXPIRED` must not occupy the unique key, so the same seat can be reserved again. A non-filtered unique index would lock the seat forever after the first insert — **do not** do that.

**Why not `WHERE IsActive = 1`:** Intake requires a **status enum**, not `IsApproved` / occupancy via `IsActive`. Do **not** add a redundant `IsActive` on `Reservation` for uniqueness.

---

### 6 — Create reservation endpoints
Create file: `Features/Reservations/ReservationEndpoints.cs`

Follow `HallEndpoints` / `ScreeningEndpoints` static class + `MapGroup` + `.WithTags("Reservations")`.

```csharp
var group = app.MapGroup("/reservations").WithTags("Reservations");
```

**Only this route in Story 06:**

`POST ""` → `POST /api/v1/reservations`  
`.RequireAuthorization()` — **not** `"AdminOnly"`.

Inject: `CreateReservationRequest request`, `AppDbContext db`, `HttpContext httpContext` (or `ClaimsPrincipal user`), `IOptions<ReservationSettings> reservationOptions`.

**User id extraction (mandatory):** parse Guid from, in order: `ClaimTypes.NameIdentifier`, `JwtRegisteredClaimNames.Sub`, claim type `"sub"`. If missing or not a Guid → `Results.Json(..., statusCode: 401)` with `{ message = "Invalid authentication token." }`. **Never** read user id from the request body.

**Handler algorithm (do this order):**

1. Resolve `userId` from claims (above).
2. Read settings; if `HoldDurationMinutes <= 0` throw (config error).
3. Load screening with hall and movie:

```csharp
var screening = await db.Screenings
    .Include(s => s.Hall)
    .Include(s => s.Movie)
    .FirstOrDefaultAsync(s => s.Id == request.ScreeningId);
```

4. If `screening` is null → `404` `{ message = "Screening not found." }`
5. If `!screening.Movie.IsActive` → `400` `{ message = "Cannot reserve a seat for an inactive movie." }`
6. If `!screening.Hall.IsActive` → `400` `{ message = "Cannot reserve a seat in an inactive hall." }`
7. If `screening.StartDateTime <= DateTime.UtcNow` → `400` `{ message = "Cannot reserve a seat for a screening that has already started." }`
8. **Seat validation against DB hall only** (never a client capacity field):
   - If `request.SeatNo < 1 || request.SeatNo > screening.Hall.NumberOfSeats` → `400` `{ message = "Seat number is invalid for this hall." }`
9. Open a transaction (`await using var tx = await db.Database.BeginTransactionAsync();`).
10. **Release expired holds for this exact seat** so the filtered unique index does not block a new hold after the deadline:

```csharp
var now = DateTime.UtcNow;
var stale = await db.Reservations
    .Where(r => r.ScreeningId == request.ScreeningId
             && r.SeatNo == request.SeatNo
             && r.Status == ReservationStatus.PENDING_PAYMENT
             && r.ExpiresAt <= now)
    .ToListAsync();
foreach (var row in stale)
{
    row.Status = ReservationStatus.EXPIRED;
    row.UpdatedAt = now;
}
```

11. Insert:

```csharp
var createdAt = now;
var reservation = new Reservation
{
    Id = Guid.NewGuid(),
    UserId = userId,
    ScreeningId = screening.Id,
    SeatNo = request.SeatNo,
    Status = ReservationStatus.PENDING_PAYMENT,
    CreatedAt = createdAt,
    ExpiresAt = createdAt.AddMinutes(reservationOptions.Value.HoldDurationMinutes),
    Amount = screening.Price,
    Currency = reservationOptions.Value.Currency
};
db.Reservations.Add(reservation);
```

12. `SaveChangesAsync` + `CommitAsync` inside `try`. On `DbUpdateException` → `RollbackAsync` if needed, then `409` `{ message = "This seat is already reserved for this screening." }` (unique index lost the race). Prefer **409** over 400 for occupancy conflicts so clients can distinguish validation errors from contention.
13. Return `201` `Results.Created($"/api/v1/reservations/{reservation.Id}", dto)` with `ReservationDto`.

**Do not** add `amount` from JSON. If someone posts extra JSON properties, the record binder ignores them; amount still comes from `screening.Price`.

---

### 7 — Register the feature
File: `Extensions/EndpointRouteBuilderExtensions.cs`

- Add `using CinemaTicketsBack.Features.Reservations;`
- After line 17 (`MapScreeningEndpoints()`), add `apiV1.MapReservationEndpoints();`

---

### 8 — EF Core migration
From repo root `.` after `dotnet build` succeeds:

```text
dotnet ef migrations add AddReservationsTable
dotnet ef database update
```

Confirm the generated `Up` includes: `Reservations` table, FKs to `Users` and `Screenings` with `ReferentialAction.Restrict` (same as `Migrations/20260822153943_AddScreeningsTable.cs` lines 29–40), `Amount` `decimal(18,2)`, `Status` string column, and unique **filtered** index `IX_Reservations_ScreeningId_SeatNo_Active`. If the scaffolded filter SQL is missing or wrong, edit the migration `CreateIndex` call to set `filter:` to `[Status] IN (N'PENDING_PAYMENT', N'CONFIRMED')`.

---

## Edge Cases & Failure Modes
- **Seat `0` / negative / greater than capacity:** Trigger: `SeatNo` 0, -5, or 101 when `Hall.NumberOfSeats` is 100. Expected: 400 `"Seat number is invalid for this hall."` Enforced in `ReservationEndpoints` create handler **after** screening+hall load (step 8). Capacity from `screening.Hall.NumberOfSeats` (`Models/Hall.cs` line 9), not the client.
- **Seat `1` and seat `N`:** Trigger: hall with `NumberOfSeats >= 1`, `SeatNo` 1 or `NumberOfSeats`. Expected: 201 if other rules pass. Same comparison `SeatNo < 1 || SeatNo > NumberOfSeats`.
- **Missing screening:** Trigger: random Guid. Expected: 404 `"Screening not found."` after `FirstOrDefaultAsync` (step 4).
- **Inactive movie / hall:** Trigger: `Movie.IsActive == false` or `Hall.IsActive == false` on the loaded graph. Expected: 400 with the messages in steps 5–6. Enforced in handler; Screening POST uses the same idea at `ScreeningEndpoints.cs` lines 51–63.
- **Screening already started:** Trigger: `StartDateTime <= DateTime.UtcNow`. Expected: 400 (step 7). This is the Story 05 “past screening cannot accept reservations” enforcement point.
- **Unauthenticated POST:** Trigger: no `Authorization` header. Expected: **401**. Enforced by `.RequireAuthorization()` plus `Program.cs` lines 24–25 (`UseAuthentication` / `UseAuthorization`).
- **Authenticated but claims lack a Guid sub:** Trigger: malformed token. Expected: 401 `"Invalid authentication token."` in the handler before DB writes.
- **Duplicate active seat same screening:** Trigger: second POST same `screeningId`+`seatNo` while first is `PENDING_PAYMENT` with `ExpiresAt > now` or `CONFIRMED`. Expected: 409 after unique index or after a pre-check if you add one. Enforced by filtered unique index (Task 5) + `DbUpdateException` (step 12). Same user, same seat, same screening: also blocked (intake rule 8).
- **Same seat, different screening:** Trigger: two screenings, both `SeatNo` 10. Expected: both 201. Unique index is `(ScreeningId, SeatNo)`, not `SeatNo` alone.
- **Expired `PENDING_PAYMENT` then re-reserve:** Trigger: hold past `ExpiresAt`, new POST same seat. Expected: 201 after step 10 sets old row to `EXPIRED` (drops them from the filtered index) then insert. If step 10 is skipped, the unique index **still blocks** — do not skip it.
- **Concurrent double-insert:** Trigger: two requests pass validation together. Expected: one 201, one 409. Enforced by unique index + `DbUpdateException`, **not** by check-then-insert alone. Same pattern as `ScreeningEndpoints.cs` lines 105–112.
- **Client sends `amount` / `currency` / `userId`:** Trigger: extra JSON. Expected: ignored; `Amount` from `screening.Price` (`Models/Screening.cs` line 9); `UserId` from JWT (`JwtTokenGenerator.cs` line 31).
- **`HoldDurationMinutes` missing or 0:** Trigger: bad `appsettings.json`. Expected: `InvalidOperationException` → `ExceptionHandlingMiddleware.cs` lines 17–42 returns 500 generic body. Do not create a reservation with `ExpiresAt == CreatedAt`.
- **Hard-delete User or Screening with reservations:** Trigger: `db.Users.Remove` / `db.Screenings.Remove`. Expected: `DbUpdateException` due to `DeleteBehavior.Restrict`. Same FK policy as Screening→Movie/Hall (`AppDbContext.cs` lines 69–79).
- **Payment confirm:** Not implemented. Reservation remains `PENDING_PAYMENT`. No endpoint may set `CONFIRMED` in this story.
- **No test project yet:** `Glob` found 0 `*Test*` files; `cinema-tickets-back.csproj` line 8 excludes `cinema-tickets-back.Tests`. Follow Story 05 test scaffolding if you add tests.

---

## Test Plan
Follow Story 05: xUnit + `WebApplicationFactory<Program>` in `cinema-tickets-back.Tests` if that project is created. Seed active movie, active hall (`NumberOfSeats = 100`), future screening with known `Price`. Obtain JWT via `POST /api/v1/auth/login` (or factory authentication).

1. **Unit — seat range helper** (extract `static bool IsValidSeatNo(int seatNo, int hallCapacity)` if it keeps tests cheap):
   - File: `cinema-tickets-back.Tests/Features/Reservations/SeatValidationTests.cs`
   - `Seat_1_Accepted` when capacity >= 1
   - `Seat_EqualToCapacity_Accepted`
   - `Seat_0_Rejected`
   - `Seat_Negative_Rejected`
   - `Seat_GreaterThanCapacity_Rejected`

2. **Integration — create reservation**
   - File: `cinema-tickets-back.Tests/Features/Reservations/ReservationEndpointsTests.cs`
   - `Create_Unauthenticated_Returns401`
   - `Create_ValidSeat_Returns201_PendingPayment_ExpiresAt_AmountFromScreeningPrice`
   - `Create_ClientAmountIgnored` — POST body with extra `amount` still uses screening price
   - `Create_PastScreening_Returns400`
   - `Create_InactiveMovie_Returns400`
   - `Create_InactiveHall_Returns400`
   - `Create_UnknownScreening_Returns404`
   - `Create_Seat0_Returns400`
   - `Create_SeatAboveCapacity_Returns400`
   - `Create_DuplicateActiveSeat_Returns409`
   - `Create_SameSeatDifferentScreening_Returns201`
   - `Create_AfterExpiredHold_Returns201` — seed `PENDING_PAYMENT` with `ExpiresAt` in the past, then POST succeeds and old row is `EXPIRED`

3. **Integration — concurrency**
   - Two parallel POSTs same screening+seat → exactly one 201, the other 409 (or one 201 and one 409/500 mapped to 409). Unique index must be present.

4. **Regression**
   - `POST /api/v1/auth/login` still 200
   - `GET /api/v1/movies/{id}/screenings` still public 200
   - `POST /api/v1/halls` still AdminOnly

---

## Migration / Rollback
- **Forward:** `dotnet build` then `dotnet ef migrations add AddReservationsTable` then `dotnet ef database update` in `.`.
- **Half-applied:** SQL Server wraps the migration in a transaction; failed `CreateTable` / index should roll back. Risk after success: app code without migration (or vice versa) → runtime SQL errors on `Reservations`.
- **Rollback:** `dotnet ef database update AddScreeningsTable` (last Story 05 migration name: `AddScreeningsTable` in `Migrations/20260822153943_AddScreeningsTable.cs`). Delete the new `Migrations/<timestamp>_AddReservationsTable.*` files and revert snapshot + application files listed in Done Criteria.
- **Filter index mistake:** If the unique index is created **without** a filter, `EXPIRED`/`CANCELLED` rows will block re-booking. Drop and recreate the index with the filter; do not ship the non-filtered unique index.

---

## Verification Steps
1. **Backend builds:** Run `dotnet build` in `.`. Zero errors. `Reservation` must be in the EF model (no “entity type not included” warning).
2. **Migration:** `dotnet ef migrations add AddReservationsTable` then `dotnet ef database update` in `.`. Confirm table `Reservations`, Restrict FKs, filtered unique index in `sys.indexes` / `sys.indexes.filter_definition`.
3. **Smoke — auth:** `dotnet run` in `.`. `POST /api/v1/reservations` with no token → **401**.
4. **Smoke — seed:** Admin JWT: create active movie, hall with `numberOfSeats: 100`, future screening with `price: 150`. Customer JWT: `POST /api/v1/reservations` `{ "screeningId": "<guid>", "seatNo": 10 }` → **201**, `status` `"PENDING_PAYMENT"`, `amount` `150`, `currency` `"EGP"`, `expiresAt` ≈ now + configured minutes, `seatNo` 10.
5. **Smoke — seats:** `seatNo` 1 and 100 → 201; `0`, `-5`, `101` → 400. Confirm hall capacity was not sent in the body.
6. **Smoke — occupancy:** second POST same screening+seat 10 → 409. Different screening, seat 10 → 201.
7. **Smoke — future-only:** screening with `StartDateTime` in the past → 400.
8. **Smoke — amount:** POST with extra `"amount": 1` still returns screening price 150.
9. **Regression:** `GET /api/v1/.../health` (mapped at `EndpointRouteBuilderExtensions.cs` line 18) still healthy; screening GET still works.
10. **Tests (if project exists):** `dotnet test cinema-tickets-back.Tests/cinema-tickets-back.Tests.csproj`

---

## Done Criteria
- [ ] `Enums/ReservationStatus.cs` exists with `PENDING_PAYMENT`, `CONFIRMED`, `CANCELLED`, `EXPIRED` and string JSON conversion. Create path only inserts `PENDING_PAYMENT` (and may mark stale rows `EXPIRED`).
- [ ] `Models/Reservation.cs` FKs `UserId` + `ScreeningId`, `SeatNo` int, no `HallSeats`, no `IsApproved`, `Amount`/`Currency`/`ExpiresAt`/`Status` present.
- [ ] `CreateReservationRequest` contains **only** `ScreeningId` and `SeatNo`. Response DTO matches intake fields (`reservationId`, `screeningId`, `seatNo`, `status`, `expiresAt`, `amount`, `currency`).
- [ ] Hold duration and currency come from `ReservationSettings` in `appsettings.json`, not literals in the handler (except entity default `"EGP"` matching config).
- [ ] `AppDbContext` has `DbSet<Reservation>` and Restrict FKs to User and Screening.
- [ ] Filtered unique index on `(ScreeningId, SeatNo)` where status is `PENDING_PAYMENT` or `CONFIRMED`.
- [ ] `POST /api/v1/reservations` registered via `MapReservationEndpoints` on the `/api/v1` group; requires JWT; does **not** use `AdminOnly`.
- [ ] Seat 1 and max seat accepted; 0, negative, and over-capacity rejected using `Hall.NumberOfSeats` loaded via the screening.
- [ ] Unauthenticated create is 401; past screening, inactive movie/hall rejected; missing screening 404.
- [ ] Duplicate active `(screening, seat)` rejected (409) including concurrent requests; same seat on another screening allowed.
- [ ] Successful create is `PENDING_PAYMENT` with `ExpiresAt` set and `Amount` equal to `Screening.Price`.
- [ ] No payment-confirm endpoint; reservation stays unconfirmed.
- [ ] EF migration `AddReservationsTable` applied; `dotnet build` succeeds.

**STOP HERE. Report to the user and wait for confirmation before proceeding to a later story (cancel / pay / expire job).**
