# cmsOrg API

**cmsOrg** is an ASP.NET Core 8 Web API that handles organisation management for a CMS platform. It covers:

- Creating and managing organisations
- Role-based access control (RBAC) within organisations
- Managing organisation members (user-role assignments)
- Managing API keys per organisation

All endpoints require a valid JWT (`Authorization: Bearer <token>`), except where noted. The user identity is resolved from the JWT `sub` / `NameIdentifier` claim — never pass `userId` explicitly.

---

## Roles & Access Control

Three roles exist, in descending order of privilege:

| Role   | Weight | Capabilities |
|--------|--------|-------------|
| Admin  | 3      | Full access — org settings, members, API keys |
| Editor | 2      | Edit content (tags, categories, entries, assets); cannot manage org |
| Viewer | 1      | Read-only |

Access checks are **hierarchical** — a required role of `Admin` blocks Editor and Viewer.

---

## Organisation Flow

1. Any authenticated user calls `POST /organisations` to create an organisation.
2. The creator is **automatically assigned the Admin role** for that organisation.
3. The Admin can add other users via `POST /organisations/{id}/members` with role `Viewer`, `Editor`, or `Admin`.
4. The Admin can change a member's role via `PUT /organisations/{id}/members/{memberId}/role`.
5. The Admin can remove members via `DELETE /organisations/{id}/members/{memberId}`.
6. Only Admins can update or delete the organisation itself, and manage API keys.

---

## Error Responses

All errors return `{ "code": int, "message": "string" }`:

| HTTP Status | Trigger |
|-------------|---------|
| 400         | Validation / bad request |
| 401         | Missing or invalid JWT |
| 403         | Insufficient role |
| 404         | Resource not found |
| 409         | Conflict (duplicate name, user already a member) |
| 500         | Unhandled exception |

---

## Endpoints

### Organisations — `/organisations`

---

#### `GET /organisations`
**Auth:** Any authenticated user

**Query parameters:**

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page |
| `search` | string? | — | Filter by name (contains) |

**Returns:** `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "createdAt": "datetime"
    }
  ],
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 0
}
```
Only returns organisations where the current user has a membership (any role).

---

#### `GET /organisations/{id}`
**Auth:** Minimum role — **Viewer**

**Route params:** `id` (Guid)

**Returns:** `200 OK`
```json
{
  "id": "guid",
  "name": "string",
  "createdAt": "datetime"
}
```
Result is cached in Redis for 10 minutes.

---

#### `POST /organisations`
**Auth:** Any authenticated user

**Body:**
```json
{ "name": "string" }
```

**Returns:** `200 OK`
```json
{
  "id": "guid",
  "name": "string",
  "createdAt": "datetime"
}
```
The caller is automatically assigned the **Admin** role for the new organisation. Returns `409 Conflict` if the user already owns an org with that name.

---

#### `PUT /organisations/{id}`
**Auth:** Minimum role — **Admin**

**Route params:** `id` (Guid)

**Body:**
```json
{ "name": "string" }
```

**Returns:** `204 No Content`

Invalidates the Redis cache for the organisation.

---

#### `DELETE /organisations/{id}`
**Auth:** Minimum role — **Admin**

**Route params:** `id` (Guid)

**Returns:** `204 No Content`

Cascades — deletes all memberships and API keys for the organisation. Invalidates Redis cache.

---

#### `GET /organisations/{id}/role`
**Auth:** Any authenticated user

**Route params:** `id` (Guid)

**Returns:** `200 OK`
```json
{ "role": "Admin" }
```
`role` is `"Admin"`, `"Editor"`, `"Viewer"`, or `null` if the user is not a member. Intended for frontends to gate UI elements.

---

#### `GET /organisations/permissions`
**Auth:** None (public — no JWT required)

**Returns:** `200 OK`
```json
[
  {
    "id": "00000000-0000-0000-0000-000000000001",
    "name": "Admin",
    "description": "Full access to everything including organisation settings."
  },
  {
    "id": "00000000-0000-0000-0000-000000000002",
    "name": "Editor",
    "description": "Can edit tags, categories, entries, and assets. Cannot edit organisation."
  },
  {
    "id": "00000000-0000-0000-0000-000000000003",
    "name": "Viewer",
    "description": "Can only view things in the organisation."
  }
]
```

---

### Members — `/organisations/{organisationId}/members`

---

#### `GET /organisations/{organisationId}/members`
**Auth:** Minimum role — **Viewer**

**Route params:** `organisationId` (Guid)

**Returns:** `200 OK`
```json
[
  {
    "id": "guid",
    "userId": "guid",
    "organisationId": "guid",
    "role": "Admin"
  }
]
```

---

#### `POST /organisations/{organisationId}/members`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid)

**Body:**
```json
{
  "userId": "guid",
  "roleTemplate": "Viewer"
}
```
Valid values for `roleTemplate`: `"Viewer"`, `"Editor"`, `"Admin"`. Defaults to `"Viewer"` if unrecognised.

**Returns:** `200 OK`
```json
{
  "id": "guid",
  "userId": "guid",
  "organisationId": "guid",
  "role": "Viewer"
}
```
Returns `404` if the organisation does not exist, `409` if the user is already a member.

---

#### `DELETE /organisations/{organisationId}/members/{id}`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid), `id` (Guid — the membership record ID)

**Returns:** `204 No Content`

Returns `404` if the membership record is not found.

---

#### `PUT /organisations/{organisationId}/members/{id}/role`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid), `id` (Guid — the membership record ID)

**Body:** plain string (the new role name)
```json
"Admin"
```

**Returns:** `204 No Content`

Returns `404` if the membership record or role name is not found.

---

### API Keys — `/organisations/{organisationId}/api-keys`

All API key endpoints require minimum role **Admin**.

---

#### `GET /organisations/{organisationId}/api-keys`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid)

**Returns:** `200 OK` (ordered newest first)
```json
[
  {
    "id": "guid",
    "organisationId": "guid",
    "key": "base64string",
    "createdAt": "datetime",
    "expiresAt": "datetime | null",
    "isActive": true
  }
]
```

---

#### `POST /organisations/{organisationId}/api-keys`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid)

**Body:**
```json
{ "expiresAt": "2027-01-01T00:00:00Z" }
```
`expiresAt` can be `null` for no expiry.

**Returns:** `200 OK`
```json
{
  "id": "guid",
  "organisationId": "guid",
  "key": "base64string",
  "createdAt": "datetime",
  "expiresAt": "datetime | null",
  "isActive": true
}
```
The key is 32 cryptographically random bytes encoded as Base64. `isActive` is always `true` on creation.

---

#### `PATCH /organisations/{organisationId}/api-keys/{id}/toggle`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid), `id` (Guid)

**Returns:** `204 No Content`

Flips `isActive` (true → false or false → true). Returns `404` if not found.

---

#### `DELETE /organisations/{organisationId}/api-keys/{id}`
**Auth:** Minimum role — **Admin**

**Route params:** `organisationId` (Guid), `id` (Guid)

**Returns:** `204 No Content`

Returns `404` if not found.

---

## Key Files

| File | Purpose |
|------|---------|
| `src/cmsOrg.API/Controllers/OrganisationController.cs` | Organisation CRUD + role/permissions endpoints |
| `src/cmsOrg.API/Controllers/UserOrganisationRoleController.cs` | Member management |
| `src/cmsOrg.API/Controllers/OrganisationApiKeyController.cs` | API key management |
| `src/cmsOrg.API/Middlewares/JwtValidationMiddleware.cs` | Validates JWT on every request |
| `src/cmsOrg.API/Middlewares/ErrorHandlingMiddleware.cs` | Maps AppException → HTTP status codes |
| `src/cmsOrg.Infrastructure/Services/OrganisationService.cs` | Organisation logic, auto-assigns Admin on create |
| `src/cmsOrg.Infrastructure/Services/AccessControlService.cs` | Role hierarchy check — throws 403 if insufficient |
| `src/cmsOrg.Infrastructure/Services/UserOrganisationRoleService.cs` | Member assign/remove/update logic |
| `src/cmsOrg.Infrastructure/Services/OrganisationApiKeyService.cs` | API key generation and management |
