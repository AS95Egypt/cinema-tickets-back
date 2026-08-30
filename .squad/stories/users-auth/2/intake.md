> **Fetched from github:** [2](https://github.com/AS95Egypt/cinema-tickets-back/issues/2)  
> *Fetched 2026-08-21T19:12:37.421Z. Edit the sections below as needed; the planner reads this file verbatim.*


## Source — work item (from tracker)

**Title:** User Story 2 — User Registration and Authentication  
**Type:** Issue  
**Status:** open

### Description

## Feature

Authentication & User Management

## User Story

As a visitor, I want to register and log in securely so that I can access the cinema reservation functionality as an authenticated user.

## Requirements

### Registration

Create:

```http
POST /api/auth/register
```

Request:

```json
{
  "username": "ahmed",
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

The system must:

1. Validate the username.
2. Validate the email format.
3. Ensure the email is unique.
4. Validate password requirements.
5. Hash the password before storing it.
6. Never store the plain-text password.
7. Create the user with `IsAdmin = false` by default.
8. Create the user as active by default.

### Login

Create:

```http
POST /api/auth/login
```

Request:

```json
{
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

The system must:

1. Find the user by email.
2. Verify the password against the stored password hash.
3. Reject inactive users.
4. Generate a JWT access token after successful authentication.
5. Include appropriate user claims/roles in the token.
6. Return the authenticated user's basic information and token.

Example response:

```json
{
  "accessToken": "<jwt>",
  "expiresIn": 3600,
  "user": {
    "id": "8a9c...",
    "username": "ahmed",
    "email": "ahmed@example.com",
    "isAdmin": false
  }
}
```

## Authorization

* Admin-only endpoints must require an administrator role/claim.
* Authenticated user endpoints must require a valid JWT.
* Public endpoints should remain accessible without authentication where explicitly specified.

## Security Requirements

* Passwords must be hashed using a secure password-hashing mechanism.
* JWT signing secrets must come from configuration/secrets and must not be committed to source control.
* Authentication errors should not reveal whether an email exists.
* Protected endpoints must validate JWT signature and expiration.

## Acceptance Criteria

* [ ] A new user can register.
* [ ] Duplicate email registration is rejected.
* [ ] Password is never stored in plain text.
* [ ] A registered user can log in with valid credentials.
* [ ] Invalid credentials are rejected.
* [ ] Inactive users cannot log in.
* [ ] Successful login returns a JWT.
* [ ] Protected endpoints reject requests without a valid JWT.
* [ ] Admin endpoints reject normal users.

### Attachments

None.

---
# Story intake

Fill this template for each story you want planned. Keep it copy-paste-friendly: the planner reads **this file and the files in `attachments/`**, nothing else.

- Folder: `.squad/stories/users-auth/2/intake.md`
- Binaries (screenshots, PDFs, exports): put them in `attachments/` next to this file and list them below.
- Do **not** rely on external links (tracker URLs, wiki, chat) — the planner cannot open them. Paste the content you want considered.

This is **not** an implementation prompt. It is the input to the plan-generation meta-prompt bundled with squad-kit (`generate-plan.md` in the installed package).

---

## Feature

- **Feature name (display):**
- **Feature slug (folder under `plans/`):** `users-auth`

## Tracker (metadata only)

- **Tracker type:** `github`
- **Work item id:** `2` *(used in filenames and plan tables; fill manually if empty)*
- **Work item type:** `Issue`
- **Status:** `open`
- **Assignee:** ``
- **Labels:** ``

External tracker links are **not** followed by the planner. Keep the id for naming and traceability only.

---

## Title

*(Paste the work item title verbatim. Prefilled when `squad new-story` fetched from a tracker.)*

```
User Story 2 — User Registration and Authentication
```

---

## Description

*(Paste the full work item description. Prefilled when fetched from a tracker.)*

```
## Feature

Authentication & User Management

## User Story

As a visitor, I want to register and log in securely so that I can access the cinema reservation functionality as an authenticated user.

## Requirements

### Registration

Create:

```http
POST /api/auth/register
```

Request:

```json
{
  "username": "ahmed",
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

The system must:

1. Validate the username.
2. Validate the email format.
3. Ensure the email is unique.
4. Validate password requirements.
5. Hash the password before storing it.
6. Never store the plain-text password.
7. Create the user with `IsAdmin = false` by default.
8. Create the user as active by default.

### Login

Create:

```http
POST /api/auth/login
```

Request:

```json
{
  "email": "ahmed@example.com",
  "password": "Password123!"
}
```

The system must:

1. Find the user by email.
2. Verify the password against the stored password hash.
3. Reject inactive users.
4. Generate a JWT access token after successful authentication.
5. Include appropriate user claims/roles in the token.
6. Return the authenticated user's basic information and token.

Example response:

```json
{
  "accessToken": "<jwt>",
  "expiresIn": 3600,
  "user": {
    "id": "8a9c...",
    "username": "ahmed",
    "email": "ahmed@example.com",
    "isAdmin": false
  }
}
```

## Authorization

* Admin-only endpoints must require an administrator role/claim.
* Authenticated user endpoints must require a valid JWT.
* Public endpoints should remain accessible without authentication where explicitly specified.

## Security Requirements

* Passwords must be hashed using a secure password-hashing mechanism.
* JWT signing secrets must come from configuration/secrets and must not be committed to source control.
* Authentication errors should not reveal whether an email exists.
* Protected endpoints must validate JWT signature and expiration.

## Acceptance Criteria

* [ ] A new user can register.
* [ ] Duplicate email registration is rejected.
* [ ] Password is never stored in plain text.
* [ ] A registered user can log in with valid credentials.
* [ ] Invalid credentials are rejected.
* [ ] Inactive users cannot log in.
* [ ] Successful login returns a JWT.
* [ ] Protected endpoints reject requests without a valid JWT.
* [ ] Admin endpoints reject normal users.
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
