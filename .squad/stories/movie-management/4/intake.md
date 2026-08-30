> **Fetched from github:** [4](https://github.com/AS95Egypt/cinema-tickets-back/issues/4)  
> *Fetched 2026-08-22T11:19:12.081Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 4 — Movie Management  
**Type:** Issue  
**Status:** open

### Description

## Feature

Movie Management

## User Story

As an administrator, I want to manage movies so that active movies can be scheduled for cinema screenings and displayed to customers.

## Movie Data

A movie contains:

```text
Id
Title
Genre
Duration
ReleaseDate
Language
Description
Actors
TrailerUrl
IsActive
CreatedAt
UpdatedAt
```

Movie ID should use UUID/GUID.

## Supported Genres

The system should define a controlled set of supported genres, for example:

```text
Comedy
Action
Drama
Fantasy
```

The exact list can be extended later.

## APIs

### Create Movie

```http
POST /api/movies
```

Example:

```json
{
  "title": "Example Movie",
  "genre": "Action",
  "duration": 120,
  "releaseDate": "2026-08-20",
  "language": "English",
  "description": "Example description",
  "actors": "Actor A, Actor B",
  "trailerUrl": "https://example.com/trailer"
}
```

### Get Movies

```http
GET /api/movies
```

The API should support retrieving movies with appropriate filtering.

### Get Movie

```http
GET /api/movies/{id}
```

### Update Movie

```http
PUT /api/movies/{id}
```

### Deactivate Movie

```http
PATCH /api/movies/{id}/deactivate
```

## Customer Movie Listing

Provide an endpoint for active movies:

```http
GET /api/movies/active
```

The response should support a summary/detailed representation as appropriate.

## Business Rules

1. Movie title is required.
2. Duration must be greater than zero.
3. Release date must be valid.
4. Trailer URL must be validated when provided.
5. Inactive movies must not appear in the customer-facing active movie list.
6. A movie with historical screenings should not be hard-deleted.
7. Deactivating a movie must not delete historical reservations.

## Technical Notes

* Use `Guid`/UUID for movie IDs.
* Use an enum or controlled lookup for movie genres.
* Store duration in minutes.
* Store URLs as strings with appropriate validation.
* Avoid storing duplicated movie duration in screening/slot records.

## Acceptance Criteria

* [ ] Admin can create a movie.
* [ ] Admin can update a movie.
* [ ] Admin can view a movie.
* [ ] Admin can deactivate a movie.
* [ ] Invalid duration is rejected.
* [ ] Inactive movies are excluded from active movie listings.
* [ ] Movie data can be retrieved for customer-facing screens.
* [ ] Historical screening information remains valid when a movie is deactivated.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/movie-management/4/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `movie-management`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `4` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 4 — Movie Management
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
## Feature

Movie Management

## User Story

As an administrator, I want to manage movies so that active movies can be scheduled for cinema screenings and displayed to customers.

## Movie Data

A movie contains:

```text
Id
Title
Genre
Duration
ReleaseDate
Language
Description
Actors
TrailerUrl
IsActive
CreatedAt
UpdatedAt
```

Movie ID should use UUID/GUID.

## Supported Genres

The system should define a controlled set of supported genres, for example:

```text
Comedy
Action
Drama
Fantasy
```

The exact list can be extended later.

## APIs

### Create Movie

```http
POST /api/movies
```

Example:

```json
{
  "title": "Example Movie",
  "genre": "Action",
  "duration": 120,
  "releaseDate": "2026-08-20",
  "language": "English",
  "description": "Example description",
  "actors": "Actor A, Actor B",
  "trailerUrl": "https://example.com/trailer"
}
```

### Get Movies

```http
GET /api/movies
```

The API should support retrieving movies with appropriate filtering.

### Get Movie

```http
GET /api/movies/{id}
```

### Update Movie

```http
PUT /api/movies/{id}
```

### Deactivate Movie

```http
PATCH /api/movies/{id}/deactivate
```

## Customer Movie Listing

Provide an endpoint for active movies:

```http
GET /api/movies/active
```

The response should support a summary/detailed representation as appropriate.

## Business Rules

1. Movie title is required.
2. Duration must be greater than zero.
3. Release date must be valid.
4. Trailer URL must be validated when provided.
5. Inactive movies must not appear in the customer-facing active movie list.
6. A movie with historical screenings should not be hard-deleted.
7. Deactivating a movie must not delete historical reservations.

## Technical Notes

* Use `Guid`/UUID for movie IDs.
* Use an enum or controlled lookup for movie genres.
* Store duration in minutes.
* Store URLs as strings with appropriate validation.
* Avoid storing duplicated movie duration in screening/slot records.

## Acceptance Criteria

* [ ] Admin can create a movie.
* [ ] Admin can update a movie.
* [ ] Admin can view a movie.
* [ ] Admin can deactivate a movie.
* [ ] Invalid duration is rejected.
* [ ] Inactive movies are excluded from active movie listings.
* [ ] Movie data can be retrieved for customer-facing screens.
* [ ] Historical screening information remains valid when a movie is deactivated.
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
