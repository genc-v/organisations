# CMS Organisations Service (cmsOrg)

The Organisations service is the **central authority** of the CMS platform. Every other service delegates two critical decisions to it: "does this user have the right role in this organisation?" and "is this public API key valid?". Nothing in the platform can enforce access control on its own — it always calls cmsOrg.

It is one of four backend services in the platform:

| Service | Repo | Responsibility |
|---|---|---|
| **Auth** (`cmsUserManagment`) | `../auth` | User accounts, login, JWT issuance, 2FA |
| **Organisations** (`cmsOrg`) | this repo | Orgs, membership, roles, API key lifecycle |
| **Content** (`cmsContentManagement`) | `../content` | Entry authoring, search, public delivery |
| **Assets** | `../assets` | S3 file uploads, asset metadata |

---

## Why this service exists

In a multi-tenant CMS, a user can belong to multiple organisations with different roles in each (Admin in one, Viewer in another). That membership state needs to live somewhere authoritative so that every other service can ask "what is this user allowed to do here?" without each service maintaining its own copy of membership data.

cmsOrg owns that data and exposes two lightweight query endpoints that every other service hits inline with requests:

1. `GET /organisations/{orgId}/role` — returns the authenticated user's role string for an org. Called by the Content service middleware on every write or read to an org-scoped route.
2. `GET /api-keys/validate` — checks whether a public API key is active and not expired, returns the `organisationId` it belongs to. Called by the Content service before serving any public read.

This keeps role logic in one place. When a role is changed or a key is revoked, the change is immediately visible to every consumer.

---

## How cmsOrg fits into the platform

```
Content Service (private API)                    Assets Service
     |                                                |
     | JWT + {organisationId} in route                | JWT + {orgId} in route
     |                                                |
     |-- GET /organisations/{orgId}/role ------------>|
     |                                   cmsOrg       |-- GET /organisations/{orgId}/role -->|
     |<-- { "role": "Editor" } ----------|            |<-- { "role": "Admin" } -------------|
     |                                   |            |
     | (allow or reject based on role)   |
     |                                   |
Content Service (public API)             |
     |                                   |
     | X-Api-Key header                  |
     |                                   |
     |-- GET /api-keys/validate -------->|
     |<-- { "organisationId": "..." } ---|
     |                                   |
     | (scope results to that org)       |
```

The JWT that both services validate is issued by the Auth service. cmsOrg trusts the same JWT secret — it does not call Auth to verify tokens, it validates them locally.

---

## Architecture

The project follows Clean Architecture with four layers:

```
cmsOrg.Domain          — entities, enums, seeded permission constants
cmsOrg.Application     — DTOs, interfaces, settings, AppException
cmsOrg.Infrastructure  — EF Core (MySQL), MongoDB, Redis, service implementations
cmsOrg.API             — controllers, middleware, DI wiring, Program.cs
```

Startup automatically runs EF Core migrations and seeds the three permission rows before the app starts accepting traffic.

---

## Databases

This service is the only one in the platform that uses three storage backends simultaneously.

### MySQL — primary relational store

Managed by EF Core. Tables:

#### `Organisations`

The tenant unit. Each organisation is a named workspace that users can be members of.

| Column | Type | Notes |
|---|---|---|
| `Id` | `GUID` PK | |
| `Name` | `string` max 256 | Required; unique per user (checked at create/update) |
| `CreatedAt` | `DateTime` | UTC, set on insert |

#### `Permissions`

The three fixed roles that exist in every deployment. Seeded at startup with deterministic GUIDs so they can be referenced by migrations.

| Id (fixed) | Name | What it allows |
|---|---|---|
| `...000001` | `Admin` | Full access including org settings, member management, and API key management |
| `...000002` | `Editor` | Can create and edit tags, categories, entries, and assets. Cannot manage the org itself |
| `...000003` | `Viewer` | Read-only access to everything in the org |

Role weight for enforcement: `Admin = 3`, `Editor = 2`, `Viewer = 1`.

#### `UserOrganisationPermissions`

The membership table — one row per user per organisation. A user can only have one role in a given org (enforced by a unique index on `(UserId, OrganisationId)`). Cascades on organisation delete.

| Column | Type | Notes |
|---|---|---|
| `Id` | `GUID` PK | |
| `UserId` | `GUID` | References a user in the Auth service (no FK — cross-service boundary) |
| `OrganisationId` | `GUID` FK | → `Organisations.Id`, cascade delete |
| `PermissionId` | `GUID` FK | → `Permissions.Id`, cascade delete |

When a user creates an organisation, a `UserOrganisationPermission` row is automatically inserted giving them the Admin role.

#### `OrganisationApiKeys`

Public API keys for frontend consumers. Keys are 32 random bytes encoded as Base64 (~44 characters). Each key is scoped to exactly one organisation.

| Column | Type | Notes |
|---|---|---|
| `Id` | `GUID` PK | |
| `OrganisationId` | `GUID` FK | → `Organisations.Id`, cascade delete |
| `Key` | `string` max 512 | Unique index. Value stored in plain text |
| `CreatedAt` | `DateTime` | UTC |
| `ExpiresAt` | `DateTime?` | Optional expiry. Null = never expires |
| `IsActive` | `bool` | Can be toggled without deleting |

Validation: a key is accepted only if `IsActive = true` AND (`ExpiresAt IS NULL` OR `ExpiresAt > UtcNow`).

### MongoDB — audit log

Every service method (create, update, delete, access check, key operations) fires a fire-and-forget insert to a MongoDB `logs` collection. The write is never awaited, so a MongoDB failure never affects the response.

Log document shape:

```json
{
  "_id": "<ObjectId>",
  "userId": "<string>",
  "organisationId": "<string | null>",
  "action": "CreateOrganisation",
  "resourceType": "Organisation",
  "resourceId": "<string | null>",
  "details": "<string | null>",
  "timestamp": "<UTC datetime>"
}
```

Actions logged include: `GetAllOrganisations`, `GetOrganisation`, `CreateOrganisation`, `UpdateOrganisation`, `DeleteOrganisation`, `CheckAccess`, `GetMyRole`, `GetOrganisationApiKeys`, `CreateOrganisationApiKey`, `ActivateApiKey`, `DeactivateApiKey`, `DeleteOrganisationApiKey`, `GetOrganisationMembers`, `AssignUserRole`, `RemoveUserRole`, `UpdateUserRole`.

### Redis — organisation cache

`GET /organisations/{id}` caches the org DTO under the key `organisation:{id}` with a 10-minute TTL. Cache is invalidated explicitly on update and delete. No other data is cached.

---

## Authentication

`JwtValidationMiddleware` runs before every request. It skips `[AllowAnonymous]` endpoints (the two anonymous ones are `GET /api-keys/validate` and `GET /organisations/permissions`). All others require a valid `Authorization: Bearer <token>` header.

The JWT is validated locally (HS256, issuer + audience + expiry checked) against `JwtSettings:Secret` — the same secret shared with the Auth service. No network call is made to validate the token.

After validation, `context.User` is populated from the claims and all downstream services read `UserId` from `ClaimTypes.NameIdentifier` or the `sub` claim.

---

## Access control model

Every write operation and most reads call `AccessControlService.CheckAccess(userId, organisationId, roleRequired)`. This looks up the `UserOrganisationPermission` for the user in the org, converts the role to a weight (Admin=3, Editor=2, Viewer=1), and throws `Forbidden` if the user's weight is below the required weight.

`GetAll` (list all orgs) filters the query to only orgs where the user has a permission entry — there is no global listing.

---

## API reference

All routes require JWT unless marked **anonymous**.

### Organisations (`/organisations`)

| Method | Path | Min role | Description |
|---|---|---|---|
| `GET` | `/organisations` | — (member only) | List orgs the caller belongs to. Params: `page`, `pageSize`, `search` |
| `GET` | `/organisations/{id}` | Viewer | Get org details. Cached in Redis 10 min |
| `POST` | `/organisations` | — (authenticated) | Create org. Caller is automatically assigned Admin |
| `PUT` | `/organisations/{id}` | Admin | Rename org. 409 if name conflicts |
| `DELETE` | `/organisations/{id}` | Admin | Hard delete org and all cascade data |
| `GET` | `/organisations/{id}/role` | — (authenticated) | Return caller's role in the org. Used by other services |
| `GET` | `/organisations/permissions` | **anonymous** | Return the list of all available permissions/roles |

**Create org payload:**
```json
{ "name": "My Workspace" }
```

**Update org payload:**
```json
{ "name": "New Name" }
```

---

### Members (`/organisations/{organisationId}/members`)

| Method | Path | Min role | Description |
|---|---|---|---|
| `GET` | `/` | Viewer | List members. Params: `page`, `pageSize` |
| `POST` | `/` | Admin | Add a user to the org with a role. 409 if already a member |
| `DELETE` | `/{id}` | Admin | Remove a user from the org (`id` is the `UserId`) |
| `PUT` | `/{id}/role` | Admin | Change a member's role (`id` is the `UserId`) |

**Assign member payload:**
```json
{
  "userId": "<uuid>",
  "roleTemplate": "Editor"
}
```
`roleTemplate` must be `"Admin"`, `"Editor"`, or `"Viewer"`. Falls back to `Viewer` if unrecognised.

---

### API Keys (`/organisations/{organisationId}/api-keys`)

All endpoints require **Admin** role.

| Method | Path | Description |
|---|---|---|
| `GET` | `/` | List all keys for the org (ordered newest first) |
| `POST` | `/` | Generate a new key. Returns the key value — store it now, it is not hidden after creation |
| `PATCH` | `/{id}/toggle` | Toggle `IsActive` (enable ↔ disable) without deleting |
| `DELETE` | `/{id}` | Permanently delete the key |

**Create key payload:**
```json
{ "expiresAt": "2027-01-01T00:00:00Z" }
```
`expiresAt` is optional. Omit for a non-expiring key.

**Key response:**
```json
{
  "id": "<uuid>",
  "organisationId": "<uuid>",
  "key": "<base64, 44 chars>",
  "createdAt": "<UTC>",
  "expiresAt": "<UTC | null>",
  "isActive": true
}
```

---

### API Key validation (`/api-keys`) — called by other services

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/api-keys/validate` | **anonymous** | Validate a key passed in `X-Api-Key` header. Returns `{ "organisationId": "<uuid>" }` or 401 |

This is not a user-facing endpoint — it is called server-to-server by the Content service on every public API request.

---

### Export (`/organisations`)

| Method | Path | Min role | Description |
|---|---|---|---|
| `GET` | `/organisations/export?format=json\|csv\|excel` | — (member only) | Export the caller's organisations |
| `GET` | `/organisations/{orgId}/members/export?format=json\|csv\|excel` | Admin | Export the org's member list |

---

## Configuration

| Key | Required | Description |
|---|---|---|
| `ConnectionStrings:MySQL` | Yes | MySQL connection string |
| `ConnectionStrings:MongoDB` | Yes | MongoDB connection string |
| `MongoDB:DatabaseName` | No | Database name (default: `cmsorg`) |
| `Redis:Connection` | Yes | Redis connection string |
| `JwtSettings:Secret` | Yes | HS256 signing key — must match the Auth service |
| `JwtSettings:Issuer` | Yes | JWT issuer claim |
| `JwtSettings:Audience` | Yes | JWT audience claim |

---

## Running locally

**Prerequisites:** Docker.

```bash
docker compose up -d
```

The compose file starts MySQL, MongoDB, and Redis with health checks before the API. The API waits for all three to be healthy.

Port mappings:
- API → `localhost:5059`
- MySQL → `localhost:33064`
- MongoDB → `localhost:27018`
- Redis → `localhost:6382`

Swagger UI is available at `http://localhost:5059/swagger`.

On first startup, EF Core migrations run automatically and the three permission rows (Admin, Editor, Viewer) are seeded. No manual setup is needed.
