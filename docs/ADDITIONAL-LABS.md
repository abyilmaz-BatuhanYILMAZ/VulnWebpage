# Additional local security exercises

The application includes three small, independent exercises beside the original XSS lab. These are intentionally vulnerable for training; do not expose them to the Internet.

## IDOR / BOLA — `/labs/authorization`

The server has three hard-coded synthetic documents:

| ID | Owner |
| --- | --- |
| 101 | alice |
| 103 | alice |
| 202 | bob |

Select **alice**, request **202** in unsafe mode, then repeat in safe mode.

- Unsafe: the lookup checks only the numeric document ID.
- Safer: the lookup also verifies that the document's owner matches the selected lab actor.
- Important limitation: the visitor selects `actor` in the URL/form; this is **not real authentication**. In a production system, the actor must be established from a trusted identity context, and authorization must be implemented consistently for every endpoint and action.
- Learn to test horizontal object access, tenant boundaries, lists, writes, and object relationships separately. A single ownership check cannot replace a complete authorization model.

## SQL injection — `/labs/sql-injection`

A new SQLite `:memory:` database with four synthetic product names is created for every request. After initial seeding, its connection is set to `PRAGMA query_only=ON`.

Search for `book`, then try the harmless boolean expression:

```text
' OR 1=1--
```

- Unsafe: the input is concatenated into a SQL `LIKE` statement. The boolean condition can cause all four fixture rows to be returned.
- Safer: input is bound to the `@pattern` parameter and treated as literal text.
- The input filter is deliberately narrow: up to 60 characters, with no semicolons, parentheses, file paths, periods, or other statement-capable syntax. These **laboratory restrictions are not a SQL injection defense**. The security control being taught is parameter binding.
- No main SQL Server tables or real data are touched by this SQL injection exercise. The project still uses its existing separate SQL Server database for the original stored XSS section.

## SSRF simulation — `/labs/ssrf`

Two virtual response fixtures are defined:

- `https://public.lab.invalid/catalog` → `PUBLIC_CATALOG_DEMO`
- `http://internal.lab.invalid/admin` → `SIMULATED_INTERNAL_ADMIN_DEMO`

The unsafe mode returns either recognized fixture based only on the submitted URL. The safer mode allows exactly the HTTPS public fixture.

**No real SSRF requests are made**: there is no HTTP client, network call, DNS lookup, redirect-following or proxy. This illustrates the destination-authorization decision only. It does not implement a general safe URL fetcher.

Production SSRF defenses also require policy for DNS rebinding, IPv4/IPv6 ranges, redirects and each redirect target, proxy behavior, egress restrictions, hostname normalization, userinfo, schemes, and request timing. The safest choice is often to avoid user-controlled URLs altogether.

## Running the exercises

```sh
cp .env.example .env
# Replace the sample MSSQL_SA_PASSWORD in .env.
docker compose up --build -d
```

Open `http://127.0.0.1:8080/labs/`.

```sh
npm install
npx playwright install chromium
npm run test:browser
docker compose down --volumes
```

The new `/labs/` routes return 404 unless `ASPNETCORE_ENVIRONMENT=Lab`; this prevents accidental use in the default Development/Production configurations. The existing XSS exercise remains the original main route and is also intentionally vulnerable.

## Expected automated checks

- BOLA: cross-owner access succeeds in unsafe mode, fails in safe mode, and own-document access succeeds.
- SQL: a boolean payload returns all fixture rows in unsafe mode and none in safe mode; normal search still works and statement separators are rejected.
- SSRF: the internal fixture is returned in unsafe mode but rejected in safe mode; the public fixture is allowed.
- Browser responses: all three exercise routes include a default strict Content Security Policy and `Cache-Control: no-store`.

A passing test suite demonstrates these specific expected outcomes, not complete security of the application.
