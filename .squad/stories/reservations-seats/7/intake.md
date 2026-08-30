> **Fetched from github:** [7](https://github.com/AS95Egypt/cinema-tickets-back/issues/7)  
> *Fetched 2026-08-28T13:10:51.882Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 7 — Reservation Availability  
**Type:** Issue  
**Status:** open

### Description

### Feature

Reservation Availability

### User Story

As a customer, I want to see which seats are available for a screening so that I can select an available seat before starting a reservation.

### API

```http
GET /api/screenings/{screeningId}/seats
```

Example response:

```json
{
  "screeningId": "screening-001",
  "hall": {
    "id": "hall-001",
    "title": "Hall 1",
    "numberOfSeats": 100
  },
  "seats": [
    {
      "seatNo": 1,
      "status": "AVAILABLE"
    },
    {
      "seatNo": 2,
      "status": "RESERVED"
    },
    {
      "seatNo": 3,
      "status": "AVAILABLE"
    }
  ]
}
```

### Business Rules

1. Seats from `1` through `NumberOfSeats` are returned.
2. Seats with an active `PENDING_PAYMENT` reservation are considered unavailable.
3. Seats with a `CONFIRMED` reservation are unavailable.
4. `CANCELLED` and `EXPIRED` reservations do not block the seat.
5. Expired `PENDING_PAYMENT` reservations should be treated as available.
6. Past screenings should not allow new reservations.

### Acceptance Criteria

* [ ] The API returns all valid seat numbers for the screening.
* [ ] Reserved seats are marked unavailable.
* [ ] Confirmed seats are marked unavailable.
* [ ] Expired holds do not permanently block seats.
* [ ] Available seats can be selected by customers.
* [ ] Invalid screening IDs return an appropriate error.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/reservations-seats/7/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `reservations-seats`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `7` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 7 — Reservation Availability
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
### Feature

Reservation Availability

### User Story

As a customer, I want to see which seats are available for a screening so that I can select an available seat before starting a reservation.

### API

```http
GET /api/screenings/{screeningId}/seats
```

Example response:

```json
{
  "screeningId": "screening-001",
  "hall": {
    "id": "hall-001",
    "title": "Hall 1",
    "numberOfSeats": 100
  },
  "seats": [
    {
      "seatNo": 1,
      "status": "AVAILABLE"
    },
    {
      "seatNo": 2,
      "status": "RESERVED"
    },
    {
      "seatNo": 3,
      "status": "AVAILABLE"
    }
  ]
}
```

### Business Rules

1. Seats from `1` through `NumberOfSeats` are returned.
2. Seats with an active `PENDING_PAYMENT` reservation are considered unavailable.
3. Seats with a `CONFIRMED` reservation are unavailable.
4. `CANCELLED` and `EXPIRED` reservations do not block the seat.
5. Expired `PENDING_PAYMENT` reservations should be treated as available.
6. Past screenings should not allow new reservations.

### Acceptance Criteria

* [ ] The API returns all valid seat numbers for the screening.
* [ ] Reserved seats are marked unavailable.
* [ ] Confirmed seats are marked unavailable.
* [ ] Expired holds do not permanently block seats.
* [ ] Available seats can be selected by customers.
* [ ] Invalid screening IDs return an appropriate error.
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
