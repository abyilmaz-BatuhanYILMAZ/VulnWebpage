# Extended exercises — virtual paths, object binding and the training hub

These exercises are intentionally vulnerable in unsafe mode. They are **not** a production file server or identity system and are never intended for Internet deployment.

## Path traversal — `/labs/path-traversal`

| Input | Unsafe outcome | Safer outcome |
| --- | --- | --- |
| `readme.txt` | Public virtual file returned | Public virtual file returned |
| `guide.txt` | Public virtual file returned | Public virtual file returned |
| `../internal/config.txt` | Simulated private fixture returned | Rejected as outside the public root |
| Unknown path | Fixture not found | Fixture not found (if still within the public root) |

The handler builds a virtual path starting from `/public/`, interprets `.` and `..` path segments and maps the normalized path to a **static dictionary**. The unsafe mode trusts the canonicalized result. The safer mode ensures that the normalized path begins with `/public/`.

This is deliberately simplified. The code never calls a file API to open a requested path. A real implementation must also consider symbolic links, case sensitivity, platform-specific path separators, Unicode/URL decoding, race conditions and filesystem access permissions. Do not treat this virtual path check as a production-ready path resolution library.

## Mass assignment — `/labs/mass-assignment`

A simple profile update accepts `displayName` and an additional `isAdmin=true` form field.

- **Unsafe:** both properties are copied into a synthetic result object.
- **Safer:** the writable-field allowlist includes only `displayName`; `isAdmin` is ignored.
- **Protection:** the POST includes an antiforgery token to avoid mixing CSRF with the binding demonstration.
- **Scope:** the synthetic profile is request-local and not persisted. No account is created and no privilege is granted.

A real web application must define separate input DTOs for user-editable fields, assign privileged properties using trusted server-side authorization rules and validate object-level access on every operation. Hiding an input on the client is not a security control.

## Practice hub — `/labs/`

The overview organizes six modules and includes an optional manual reviewed-checkbox per module.

- The browser-only key is `vulnwebpage.practice.v1`.
- Stored values are limited to a fixed allowlist of module IDs.
- Only a module ID list is saved, without personal information or authentication.
- Storage failures are tolerated; the page still functions during the current visit.
- The **Reset progress** control clears the saved marks.
- Progress is **not a score**, proof of exploitation, server-side verification or an achievement issued by the project.

## Isolation and test expectations

Running the application requires `ASPNETCORE_ENVIRONMENT=Lab`. Starting with any other environment value throws before the database connection is initialized. Use the loopback Docker configuration and do not connect a public reverse proxy.

The automated browser tests verify:

1. The virtual file fixture can be accessed in unsafe mode.
2. The same traversal attempt is denied in safer mode.
3. Ordinary public virtual documents still work.
4. Synthetic elevated role binding occurs in unsafe mode.
5. The safer mode rejects that extra field.
6. An invalid mass-assignment POST without an antiforgery token is rejected.
7. Completion marks persist within one browser and can be reset.
8. The overview exposes the six modules without disturbing existing XSS tests.

Pass/fail outcomes apply to these specific assertions only.
