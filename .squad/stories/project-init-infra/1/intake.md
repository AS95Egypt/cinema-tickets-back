> **Fetched from github:** [1](https://github.com/AS95Egypt/cinema-tickets-back/issues/1)  
> *Fetched 2026-08-21T17:44:18.788Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 1 — Initialize Cinema Reservation API  
**Type:** Issue  
**Status:** open

### Description

## Feature

Project Initialization & Infrastructure

## User Story

As a developer, I want to initialize the Cinema Ticket Reservation backend with a clean ASP.NET Core architecture and SQL Server database so that all subsequent features can be developed on a consistent foundation.

## Requirements

1. Create an ASP.NET Core Web API project.
2. Use SQL Server as the relational database.
3. Use Entity Framework Core as the ORM.
4. Configure dependency injection using the built-in ASP.NET Core DI container.
5. Configure application settings using `appsettings.json` and environment-specific configuration.
6. Configure a connection string for SQL Server.
7. Create the initial database context.
8. Enable EF Core migrations.
9. Configure API controllers and routing.
10. Enable Swagger/OpenAPI for development.
11. Configure global exception handling.
12. Configure structured logging using the ASP.NET Core logging infrastructure.
13. Configure development and production environments.
14. Add a health-check endpoint for the API/database where appropriate.

## Suggested Project Structure

```text
CinemaReservation/
├── Controllers/
├── Services/
├── Repositories/
├── Data/
├── Models/
├── DTOs/
├── Entities/
├── Enums/
├── Middleware/
├── Migrations/
└── Program.cs
```

The exact architecture can be refined during the execution plan.

## Technical Constraints

* ASP.NET Core Web API
* C#
* SQL Server
* Entity Framework Core
* RESTful HTTP APIs
* Swagger/OpenAPI
* Dependency Injection
* EF Core Code First migrations

## Acceptance Criteria

* [ ] The application starts successfully.
* [ ] The API can connect to SQL Server.
* [ ] EF Core migrations can be created and applied.
* [ ] Swagger is available in the development environment.
* [ ] A health-check endpoint can verify application/database availability.
* [ ] Unhandled exceptions are returned using a consistent API error format.
* [ ] Configuration does not contain hard-coded database credentials.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/project-init-infra/1/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `project-init-infra`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `1` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 1 — Initialize Cinema Reservation API
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
## Feature

Project Initialization & Infrastructure

## User Story

As a developer, I want to initialize the Cinema Ticket Reservation backend with a clean ASP.NET Core architecture and SQL Server database so that all subsequent features can be developed on a consistent foundation.

## Requirements

1. Create an ASP.NET Core Web API project.
2. Use SQL Server as the relational database.
3. Use Entity Framework Core as the ORM.
4. Configure dependency injection using the built-in ASP.NET Core DI container.
5. Configure application settings using `appsettings.json` and environment-specific configuration.
6. Configure a connection string for SQL Server.
7. Create the initial database context.
8. Enable EF Core migrations.
9. Configure API controllers and routing.
10. Enable Swagger/OpenAPI for development.
11. Configure global exception handling.
12. Configure structured logging using the ASP.NET Core logging infrastructure.
13. Configure development and production environments.
14. Add a health-check endpoint for the API/database where appropriate.

## Suggested Project Structure

```text
CinemaReservation/
├── Controllers/
├── Services/
├── Repositories/
├── Data/
├── Models/
├── DTOs/
├── Entities/
├── Enums/
├── Middleware/
├── Migrations/
└── Program.cs
```

The exact architecture can be refined during the execution plan.

## Technical Constraints

* ASP.NET Core Web API
* C#
* SQL Server
* Entity Framework Core
* RESTful HTTP APIs
* Swagger/OpenAPI
* Dependency Injection
* EF Core Code First migrations

## Acceptance Criteria

* [ ] The application starts successfully.
* [ ] The API can connect to SQL Server.
* [ ] EF Core migrations can be created and applied.
* [ ] Swagger is available in the development environment.
* [ ] A health-check endpoint can verify application/database availability.
* [ ] Unhandled exceptions are returned using a consistent API error format.
* [ ] Configuration does not contain hard-coded database credentials.
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
