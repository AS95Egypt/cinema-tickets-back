> **Fetched from github:** [3](https://github.com/AS95Egypt/cinema-tickets-back/issues/3)  
> *Fetched 2026-08-22T09:50:14.322Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 3 — Cinema Hall Management  
**Type:** Issue  
**Status:** open

### Description

## Feature

Hall Management

## User Story

As an administrator, I want to manage cinema halls and their seating capacity so that movies can be scheduled in available halls.

## Hall Data

A hall contains:

```text
Id
Title
NumberOfSeats
Type
IsActive
CreatedAt
UpdatedAt
```

Hall type must be restricted to:

```text
Standard
4D
Gold
MAX
IMAX
```

## APIs

### Create Hall

```http
POST /api/halls
```

Example:

```json
{
  "title": "Hall 1",
  "numberOfSeats": 120,
  "type": "IMAX"
}
```

### Get Halls

```http
GET /api/halls
```

The API should support filtering active/inactive halls where appropriate.

### Get Hall

```http
GET /api/halls/{id}
```

### Update Hall

```http
PUT /api/halls/{id}
```

### Deactivate Hall

Instead of physically deleting a hall, deactivate it.

```http
PATCH /api/halls/{id}/deactivate
```

## Business Rules

1. Hall title must be required.
2. Number of seats must be greater than zero.
3. Hall type must be one of the supported types.
4. A hall must not be hard-deleted if it has historical or scheduled screenings.
5. Deactivated halls cannot be used for new movie slots.
6. Existing historical reservations must continue to reference the hall.

## Technical Notes

* Hall type should be represented as an enum.
* Hall IDs should use a consistent identifier strategy.
* Database constraints should enforce required fields where appropriate.


## Acceptance Criteria

* [ ] Admin can create a hall.
* [ ] Admin can view halls.
* [ ] Admin can update a hall.
* [ ] Admin can deactivate a hall.
* [ ] Invalid hall types are rejected.
* [ ] Zero/negative seat counts are rejected.
* [ ] Deactivated halls cannot receive new screenings.
* [ ] Historical reservations remain valid after hall deactivation.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/hall-management/3/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `hall-management`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `3` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 3 — Cinema Hall Management
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
## Feature

Hall Management

## User Story

As an administrator, I want to manage cinema halls and their seating capacity so that movies can be scheduled in available halls.

## Hall Data

A hall contains:

```text
Id
Title
NumberOfSeats
Type
IsActive
CreatedAt
UpdatedAt
```

Hall type must be restricted to:

```text
Standard
4D
Gold
MAX
IMAX
```

## APIs

### Create Hall

```http
POST /api/halls
```

Example:

```json
{
  "title": "Hall 1",
  "numberOfSeats": 120,
  "type": "IMAX"
}
```

### Get Halls

```http
GET /api/halls
```

The API should support filtering active/inactive halls where appropriate.

### Get Hall

```http
GET /api/halls/{id}
```

### Update Hall

```http
PUT /api/halls/{id}
```

### Deactivate Hall

Instead of physically deleting a hall, deactivate it.

```http
PATCH /api/halls/{id}/deactivate
```

## Business Rules

1. Hall title must be required.
2. Number of seats must be greater than zero.
3. Hall type must be one of the supported types.
4. A hall must not be hard-deleted if it has historical or scheduled screenings.
5. Deactivated halls cannot be used for new movie slots.
6. Existing historical reservations must continue to reference the hall.

## Technical Notes

* Hall type should be represented as an enum.
* Hall IDs should use a consistent identifier strategy.
* Database constraints should enforce required fields where appropriate.


## Acceptance Criteria

* [ ] Admin can create a hall.
* [ ] Admin can view halls.
* [ ] Admin can update a hall.
* [ ] Admin can deactivate a hall.
* [ ] Invalid hall types are rejected.
* [ ] Zero/negative seat counts are rejected.
* [ ] Deactivated halls cannot receive new screenings.
* [ ] Historical reservations remain valid after hall deactivation.
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
