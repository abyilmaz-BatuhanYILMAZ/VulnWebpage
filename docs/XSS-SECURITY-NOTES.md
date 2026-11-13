# XSS lab — security notes

This repository intentionally contains vulnerable rendering patterns. Use it only as a local, disposable training environment.

## Test matrix

| Scenario | Deliberately unsafe sink | Safer sink |
| --- | --- | --- |
| Reflected | `Html.Raw(Model.ReflectedInput)` | `@Model.ReflectedInput` |
| Stored | `Html.Raw(item.Content)` | `@item.Content` |
| DOM | `innerHTML` | `textContent` |

The same payload can be compared with CSP off at `/?csp=off` and with enforced CSP at `/?csp=strict`.

### What CSP changes

- Razor encoding prevents HTML text from being interpreted as markup; encoding rules vary between HTML text, attribute, URL, CSS and JavaScript contexts.
- `Html.Raw` bypasses Razor's normal output encoding. It is not appropriate for arbitrary user input.
- `textContent` inserts plain text; `innerHTML` parses a string as HTML.
- The strict CSP uses `script-src 'self'` to block inline scripts and event handlers. The DOM exercise is loaded from the same-origin `wwwroot/js/xss-lab.js` so it keeps working.
- **CSP does not repair an HTML injection bug**. The unsafe sink still injects an element under strict CSP; only its inline JavaScript execution is blocked. CSP is defense in depth, not a replacement for output encoding.
- The lab allows a Bootstrap CDN stylesheet in `style-src`. Scripts are restricted to the same origin. In a production application, external resources and CSP need review with explicit threat assumptions.

## Automated verification

With the container stack running:

```sh
npm install
npx playwright install chromium
npm run test:browser
```

The test suite checks reflected script execution, strict CSP blocking, DOM safe/unsafe output, and stored payload persistence. CI runs the same tests on a temporary SQL Server database.

## Isolation

- The web server publishes only `127.0.0.1:8080` on the host.
- SQL Server does not publish port 1433 on the host.
- The `.env` file contains a local, non-reusable database password and must not be committed.
- The lab does not handle real customer data. Do not use production secrets or production databases.
- Remove the lab and its database using `docker compose down --volumes`.
- Keep the service offline or loopback-only. The intentionally vulnerable endpoints must not be exposed publicly.

Automated checks cover expected examples; they are not a general security certification.
