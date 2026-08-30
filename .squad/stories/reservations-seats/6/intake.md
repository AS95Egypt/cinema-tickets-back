> **Fetched from github:** [6](https://github.com/AS95Egypt/cinema-tickets-back/issues/6)  
> *Fetched 2026-08-24T19:34:41.166Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 6 — Validate Seats & Create reservation  
**Type:** Issue  
**Status:** open

### Description

# Stories 6–7 — Seat Validation, Create Reservation

### Feature

Seat Selection & Validation
Create Reservation

### User Story

As a customer, I want to select a valid seat for a movie screening so that I can reserve an available seat.
I want to reserve a seat for a future movie screening so that the seat is temporarily held while I complete payment.

### Business Rules

1. A hall does not have a separate `HallSeats` table.
2. Seats are represented by numeric values.
3. For a hall with `NumberOfSeats = N`, valid seat numbers are:

   * `1`
   * through
   * `N`
4. Seat number `0` or negative values are invalid.
5. A seat number greater than the hall's capacity is invalid.
6. A reservation must reference the screening rather than only the movie or hall.
7. The hall is determined from the screening.
8. A seat can only have one active reservation for a specific screening.
9. The same numeric seat can be reserved for different screenings.

### Example

For:

```text
Hall: Hall 1
NumberOfSeats: 100
```

Valid:

```text
1
50
100
```

Invalid:

```text
0
101
-5
```

```http
POST /api/reservations
Authorization: Bearer <jwt>
Content-Type: application/json
```

Request:

```json
{
  "screeningId": "screening-001",
  "seatNo": 10
}
```

### Business Rules

1. The user must be authenticated.
2. The screening must exist.
3. The movie associated with the screening must be active.
4. The hall associated with the screening must be active.
5. The screening must not have started.
6. The requested seat number must be within the hall capacity.
7. The seat must not already be actively reserved for the screening.
8. A user cannot create multiple active reservations for the same screening and seat.
9. A successful reservation initially enters `PENDING_PAYMENT`.
10. The reservation must have a payment deadline/expiration time.
11. The reservation should not be considered confirmed until payment succeeds.


### Technical Requirements

* Validate `SeatNo` against the `NumberOfSeats` of the hall associated with the screening.
* Do not trust the seat capacity supplied by the client.
* Retrieve the screening and hall from the database before validating the seat.
* Prevent duplicate active reservations for the same `(ScreeningId, SeatNo)`.
* The database should provide an appropriate uniqueness constraint/index where possible.

### Reservation Status

Use a status enum rather than `IsApproved`.

Suggested states:

```text
PENDING_PAYMENT
CONFIRMED
CANCELLED
EXPIRED
```

### Temporary Hold

When the reservation is created:

```text
Seat
AVAILABLE
   ↓
PENDING_PAYMENT
```

The system should assign an expiration time, for example:

```text
ExpiresAt = CreatedAt + configurable hold duration
```

The exact duration should be configurable rather than hard-coded.

### Example Response

```json
{
  "reservationId": "reservation-001",
  "screeningId": "screening-001",
  "seatNo": 10,
  "status": "PENDING_PAYMENT",
  "expiresAt": "2026-08-22T20:15:00Z",
  "amount": 150,
  "currency": "EGP"
}
```

### Concurrency Requirement

Two users may attempt to reserve the same seat at approximately the same time.

The system must guarantee that only one reservation can successfully hold the seat.

This must not rely only on:

```text
Check availability
    ↓
Insert reservation
```

because two requests can pass the check simultaneously.

The implementation should use appropriate database transaction/concurrency mechanisms and database constraints.

### Acceptance Criteria (Seats validation)

* [ ] Seat `1` is accepted for a hall with at least one seat.
* [ ] The hall's maximum seat number is accepted.
* [ ] Seat `0` is rejected.
* [ ] Negative seat numbers are rejected.
* [ ] A seat greater than the hall capacity is rejected.
* [ ] The system determines the hall capacity from the screening.
* [ ] A seat already reserved for the same screening cannot be reserved again.
* [ ] The same seat number can be reserved for another screening.

### Acceptance Criteria  (Craete validation)

* [ ] Unauthenticated users cannot create reservations.
* [ ] A reservation can only be created for a future screening.
* [ ] Invalid seat numbers are rejected.
* [ ] An already-held/reserved seat cannot be reserved by another user.
* [ ] A valid reservation enters `PENDING_PAYMENT`.
* [ ] The reservation receives an expiration time.
* [ ] The reservation amount is derived from the screening price.
* [ ] The client cannot override the reservation amount.
* [ ] Concurrent requests cannot successfully reserve the same screening/seat twice.
* [ ] A reservation remains unconfirmed until payment succeeds.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/reservations-seats/6/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `reservations-seats`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `6` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 6 — Validate Seats & Create reservation
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
# Stories 6–7 — Seat Validation, Create Reservation

### Feature

Seat Selection & Validation
Create Reservation

### User Story

As a customer, I want to select a valid seat for a movie screening so that I can reserve an available seat.
I want to reserve a seat for a future movie screening so that the seat is temporarily held while I complete payment.

### Business Rules

1. A hall does not have a separate `HallSeats` table.
2. Seats are represented by numeric values.
3. For a hall with `NumberOfSeats = N`, valid seat numbers are:

   * `1`
   * through
   * `N`
4. Seat number `0` or negative values are invalid.
5. A seat number greater than the hall's capacity is invalid.
6. A reservation must reference the screening rather than only the movie or hall.
7. The hall is determined from the screening.
8. A seat can only have one active reservation for a specific screening.
9. The same numeric seat can be reserved for different screenings.

### Example

For:

```text
Hall: Hall 1
NumberOfSeats: 100
```

Valid:

```text
1
50
100
```

Invalid:

```text
0
101
-5
```

```http
POST /api/reservations
Authorization: Bearer <jwt>
Content-Type: application/json
```

Request:

```json
{
  "screeningId": "screening-001",
  "seatNo": 10
}
```

### Business Rules

1. The user must be authenticated.
2. The screening must exist.
3. The movie associated with the screening must be active.
4. The hall associated with the screening must be active.
5. The screening must not have started.
6. The requested seat number must be within the hall capacity.
7. The seat must not already be actively reserved for the screening.
8. A user cannot create multiple active reservations for the same screening and seat.
9. A successful reservation initially enters `PENDING_PAYMENT`.
10. The reservation must have a payment deadline/expiration time.
11. The reservation should not be considered confirmed until payment succeeds.


### Technical Requirements

* Validate `SeatNo` against the `NumberOfSeats` of the hall associated with the screening.
* Do not trust the seat capacity supplied by the client.
* Retrieve the screening and hall from the database before validating the seat.
* Prevent duplicate active reservations for the same `(ScreeningId, SeatNo)`.
* The database should provide an appropriate uniqueness constraint/index where possible.

### Reservation Status

Use a status enum rather than `IsApproved`.

Suggested states:

```text
PENDING_PAYMENT
CONFIRMED
CANCELLED
EXPIRED
```

### Temporary Hold

When the reservation is created:

```text
Seat
AVAILABLE
   ↓
PENDING_PAYMENT
```

The system should assign an expiration time, for example:

```text
ExpiresAt = CreatedAt + configurable hold duration
```

The exact duration should be configurable rather than hard-coded.

### Example Response

```json
{
  "reservationId": "reservation-001",
  "screeningId": "screening-001",
  "seatNo": 10,
  "status": "PENDING_PAYMENT",
  "expiresAt": "2026-08-22T20:15:00Z",
  "amount": 150,
  "currency": "EGP"
}
```

### Concurrency Requirement

Two users may attempt to reserve the same seat at approximately the same time.

The system must guarantee that only one reservation can successfully hold the seat.

This must not rely only on:

```text
Check availability
    ↓
Insert reservation
```

because two requests can pass the check simultaneously.

The implementation should use appropriate database transaction/concurrency mechanisms and database constraints.

### Acceptance Criteria (Seats validation)

* [ ] Seat `1` is accepted for a hall with at least one seat.
* [ ] The hall's maximum seat number is accepted.
* [ ] Seat `0` is rejected.
* [ ] Negative seat numbers are rejected.
* [ ] A seat greater than the hall capacity is rejected.
* [ ] The system determines the hall capacity from the screening.
* [ ] A seat already reserved for the same screening cannot be reserved again.
* [ ] The same seat number can be reserved for another screening.

### Acceptance Criteria  (Craete validation)

* [ ] Unauthenticated users cannot create reservations.
* [ ] A reservation can only be created for a future screening.
* [ ] Invalid seat numbers are rejected.
* [ ] An already-held/reserved seat cannot be reserved by another user.
* [ ] A valid reservation enters `PENDING_PAYMENT`.
* [ ] The reservation receives an expiration time.
* [ ] The reservation amount is derived from the screening price.
* [ ] The client cannot override the reservation amount.
* [ ] Concurrent requests cannot successfully reserve the same screening/seat twice.
* [ ] A reservation remains unconfirmed until payment succeeds.
```

---

## Acceptance criteria

*(Checklist, bullets, Gherkin, etc. Prefilled for Azure DevOps when the work item has acceptance criteria.)*

```

```

---

## Attachments

Place files in `attachments/` next to this `intake.md`, then list them here so the planner knows what to open.

| File (relative to this folder) | What it is |
| ------------------------------ | ---------- |
| *(e.g. `attachments/flow.png`)* | *(e.g. UX flow)* |

*(Add rows per file. If none, write "None.")*

---

## Dependencies

- **Blocked by / related ids:** (tracker ids only; optional short note)
- **Depends on code areas or other stories:**

## Extra notes (optional)

- Anything not captured above (e.g. chat context) — keep short.

## Technical hints (optional)

- APIs, screens, services already discussed. Repos/roots: `.`. Primary language: `typescript`.

## Out of scope

- What this story explicitly does **not** cover:
