# cmsOrg API

All endpoints require a valid JWT (`Authorization: Bearer <token>`), except where noted. The user identity is resolved from the JWT — no need to pass `userId` explicitly.

---

## Roles & Access Control

Three roles exist, in descending order of privilege:

| Role   | Weight |
|--------|--------|
| Admin  | 3      |
| Editor | 2      |
| Viewer | 1      |

Access checks are **hierarchical** — a required role of `Admin` blocks both Editor and Viewer.

**Who can do what:**

| Action                        | Minimum role     |
|-------------------------------|------------------|
| Create organisation           | Any auth user    |
| Read organisation(s)          | Any auth user    |
| Update / Delete organisation  | Admin            |
| List members                  | Any auth user    |
| Add / Remove / Change member  | Admin            |
| Manage assets                 | Any auth user    |
| Manage API keys               | Any auth user    |

---

## Organisation Flow

1. Any authenticated user calls `POST /organisations` to create an organisation.
2. The creator is **automatically assigned the Admin role** for that organisation.
3. The Admin can add other users via `POST /organisations/{id}/members` with role `Viewer`, `Editor`, or `Admin`.
4. The Admin can change a member's role via `PUT /organisations/{id}/members/{memberId}/role`.
5. The Admin can remove members via `DELETE /organisations/{id}/members/{memberId}`.
6. Only Admins can update or delete the organisation itself.

---

## Endpoints

### Organisations — `/organisations`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET    | `/organisations` | Any | Paginated list. Query: `page`, `pageSize`, `search` |
| GET    | `/organisations/{id}` | Any | Get single organisation |
| POST   | `/organisations` | Any | Create organisation. Body: `{ "name": "string" }`. Creator becomes Admin automatically. |
| PUT    | `/organisations/{id}` | Admin | Update organisation. Body: `{ "name": "string" }` |
| DELETE | `/organisations/{id}` | Admin | Delete organisation |
| GET    | `/organisations/{id}/role` | Any | Returns the current user's role in the organisation. Response: `{ "role": "Admin\|Editor\|Viewer\|null" }`. Use this to display the user's status or gate UI elements. |
| GET    | `/organisations/permissions` | Public (no auth) | Returns the default permission list |

---

### Members — `/organisations/{organisationId}/members`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET    | `/organisations/{id}/members` | Any | List all members and their roles |
| POST   | `/organisations/{id}/members` | Admin | Assign a user to the organisation. Body: `{ "userId": "guid", "organisationId": "guid", "roleTemplate": "Viewer\|Editor\|Admin" }` |
| DELETE | `/organisations/{id}/members/{memberId}` | Admin | Remove a member |
| PUT    | `/organisations/{id}/members/{memberId}/role` | Admin | Change a member's role. Body: `"Admin"` (plain string) |

---

### Assets — `/organisations/{organisationId}/assets`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET    | `/organisations/{id}/assets` | Any | Paginated assets. Query: `page`, `pageSize` |
| GET    | `/organisations/{id}/assets/{assetId}` | Any | Get single asset |
| POST   | `/organisations/{id}/assets` | Any | Create asset. Body: `{ "name": "string", "type": "string" }` |
| PUT    | `/organisations/{id}/assets/{assetId}` | Any | Update asset. Body: `{ "name": "string", "type": "string" }` |
| DELETE | `/organisations/{id}/assets/{assetId}` | Any | Delete asset |

---

### API Keys — `/organisations/{organisationId}/api-keys`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET    | `/organisations/{id}/api-keys` | Any | List API keys for the organisation |
| POST   | `/organisations/{id}/api-keys` | Any | Create API key. Body: `{ "expiresAt": "datetime\|null" }` |
| PATCH  | `/organisations/{id}/api-keys/{keyId}/toggle` | Any | Toggle key active/inactive |
| DELETE | `/organisations/{id}/api-keys/{keyId}` | Any | Delete API key |

---

## Key Files

| File | Purpose |
|------|---------|
| `src/cmsOrg.API/Controllers/OrganisationController.cs` | Organisation CRUD + check-access endpoint |
| `src/cmsOrg.API/Controllers/UserOrganisationRoleController.cs` | Member management |
| `src/cmsOrg.API/Controllers/OrganisationAssetController.cs` | Asset management |
| `src/cmsOrg.API/Controllers/OrganisationApiKeyController.cs` | API key management |
| `src/cmsOrg.Infrastructure/Services/OrganisationService.cs` | Organisation logic, auto-assigns Admin on create |
| `src/cmsOrg.Infrastructure/Services/AccessControlService.cs` | Role hierarchy check — throws 403 if insufficient |
| `src/cmsOrg.Infrastructure/Services/UserOrganisationRoleService.cs` | Member assign/remove/update logic |
