> **Fetched from github:** [5](https://github.com/AS95Egypt/cinema-tickets-back/issues/5)  
> *Fetched 2026-08-22T15:20:57.460Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 5 — Movie Screening Management  
**Type:** Issue  
**Status:** open

### Description

## Feature

Movie Screening / Schedule Management

## User Story

As an administrator, I want to schedule movies in cinema halls at specific dates and times so that customers can view available screenings and reserve seats.

## Screening Data

A screening contains:

```text
Id
MovieId
HallId
StartDateTime
Price
CreatedAt
UpdatedAt
```

The screening duration is derived from the associated movie's duration and should not be duplicated unless there is an explicit business requirement to preserve historical duration.

## API

Create a screening:

```http
POST /api/movies/{movieId}/screenings
```

Example:

```json
{
  "hallId": "hall-001",
  "startDateTime": "2026-08-25T20:00:00",
  "price": 150
}
```

Retrieve screenings:

```http
GET /api/movies/{movieId}/screenings
```

## Business Rules

### Movie availability

A screening can only be created for an active movie.

### Hall availability

A screening can only use an active hall.

### Screening conflict

A hall cannot have overlapping screenings.

For example:

```text
Movie A
20:00 → 22:00

Movie B
21:30 → 23:30
```

This must be rejected because the screenings overlap.

The system should calculate the end time using:

```text
EndTime = StartTime + Movie.Duration
```

### Same movie

The same movie may have multiple screenings on the same day provided they do not conflict with the hall's schedule.

### Past screenings

A screening whose start time has passed cannot accept new reservations.

## Customer View

Customers should be able to retrieve:

```http
GET /api/movies/active
```

with screening information, or retrieve:

```http
GET /api/movies/{movieId}/screenings
```

The UI should be able to group screenings by day.

Example response:

```json
{
  "movieId": "movie-001",
  "title": "Example Movie",
  "screenings": [
    {
      "id": "screening-001",
      "startDateTime": "2026-08-25T18:00:00",
      "price": 150,
      "hall": {
        "id": "hall-001",
        "title": "Hall 1",
        "type": "IMAX"
      }
    },
    {
      "id": "screening-002",
      "startDateTime": "2026-08-25T21:00:00",
      "price": 150,
      "hall": {
        "id": "hall-001",
        "title": "Hall 1",
        "type": "IMAX"
      }
    }
  ]
}
```

## Acceptance Criteria

* [ ] Admin can create a screening.
* [ ] Admin can retrieve screenings for a movie.
* [ ] Inactive movies cannot receive new screenings.
* [ ] Inactive halls cannot receive new screenings.
* [ ] Overlapping screenings in the same hall are rejected.
* [ ] Screening duration is calculated from the movie duration.
* [ ] Customers can retrieve future screenings.
* [ ] Screenings can be grouped by day by the frontend.
* [ ] Past screenings cannot accept new reservations.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/movie-screening/5/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `movie-screening`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `5` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 5 — Movie Screening Management
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
## Feature

Movie Screening / Schedule Management

## User Story

As an administrator, I want to schedule movies in cinema halls at specific dates and times so that customers can view available screenings and reserve seats.

## Screening Data

A screening contains:

```text
Id
MovieId
HallId
StartDateTime
Price
CreatedAt
UpdatedAt
```

The screening duration is derived from the associated movie's duration and should not be duplicated unless there is an explicit business requirement to preserve historical duration.

## API

Create a screening:

```http
POST /api/movies/{movieId}/screenings
```

Example:

```json
{
  "hallId": "hall-001",
  "startDateTime": "2026-08-25T20:00:00",
  "price": 150
}
```

Retrieve screenings:

```http
GET /api/movies/{movieId}/screenings
```

## Business Rules

### Movie availability

A screening can only be created for an active movie.

### Hall availability

A screening can only use an active hall.

### Screening conflict

A hall cannot have overlapping screenings.

For example:

```text
Movie A
20:00 → 22:00

Movie B
21:30 → 23:30
```

This must be rejected because the screenings overlap.

The system should calculate the end time using:

```text
EndTime = StartTime + Movie.Duration
```

### Same movie

The same movie may have multiple screenings on the same day provided they do not conflict with the hall's schedule.

### Past screenings

A screening whose start time has passed cannot accept new reservations.

## Customer View

Customers should be able to retrieve:

```http
GET /api/movies/active
```

with screening information, or retrieve:

```http
GET /api/movies/{movieId}/screenings
```

The UI should be able to group screenings by day.

Example response:

```json
{
  "movieId": "movie-001",
  "title": "Example Movie",
  "screenings": [
    {
      "id": "screening-001",
      "startDateTime": "2026-08-25T18:00:00",
      "price": 150,
      "hall": {
        "id": "hall-001",
        "title": "Hall 1",
        "type": "IMAX"
      }
    },
    {
      "id": "screening-002",
      "startDateTime": "2026-08-25T21:00:00",
      "price": 150,
      "hall": {
        "id": "hall-001",
        "title": "Hall 1",
        "type": "IMAX"
      }
    }
  ]
}
```

## Acceptance Criteria

* [ ] Admin can create a screening.
* [ ] Admin can retrieve screenings for a movie.
* [ ] Inactive movies cannot receive new screenings.
* [ ] Inactive halls cannot receive new screenings.
* [ ] Overlapping screenings in the same hall are rejected.
* [ ] Screening duration is calculated from the movie duration.
* [ ] Customers can retrieve future screenings.
* [ ] Screenings can be grouped by day by the frontend.
* [ ] Past screenings cannot accept new reservations.
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
