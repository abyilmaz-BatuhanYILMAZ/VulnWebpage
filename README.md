# VulnWebPage — XSS training lab

ASP.NET Core MVC (.NET 10 LTS) examples comparing vulnerable and safer output rendering. This project is **intentionally vulnerable** and intended only for local, isolated security education.

## Exercises

| Topic | Unsafe mode | Safer mode | URL |
| --- | --- | --- | --- |
| Reflected XSS | Raw HTML rendering | Razor HTML encoding | `/?csp=off` |
| Stored XSS | Raw stored HTML rendering | Razor HTML encoding | `/?csp=off` |
| DOM XSS | `innerHTML` | `textContent` | `/?csp=off` |
| CSP | Script execution permitted in the demonstration | Inline script restricted | `/?csp=strict` |
| IDOR / BOLA | Document ID lookup without owner check | Compare owner with selected lab actor | `/labs/authorization` |
| SQL injection | SQL string concatenation | Parameterized SQL | `/labs/sql-injection` |
| SSRF simulation | Any recognized destination fixture | Strict HTTPS destination allowlist | `/labs/ssrf` |

The IDOR actor is **not real authentication**, the SQL injection database is disposable and in-memory, and the SSRF exercise makes **no network requests**. See [additional exercises](docs/ADDITIONAL-LABS.md) for the threat models and limitations.

CSP enforcement blocks inline JavaScript in the example but **does not fix unsafe rendering**.

## Docker setup

Requires Docker Compose, a modern browser and enough available memory for SQL Server.

1. Copy `.env.example` to `.env` and replace the sample password with a strong, unique local value.
2. Start:

```sh
docker compose up --build -d
```

3. Open [the local XSS lab](http://127.0.0.1:8080/?csp=off), then select **CSP Enforced** to compare outcomes. Open [additional exercises](http://127.0.0.1:8080/labs/) for IDOR/BOLA, SQL injection, and the simulated SSRF scenario.
4. Stop and remove the disposable database:

```sh
docker compose down --volumes
```

The website is published only on the host loopback interface, while SQL Server remains internal to the Docker network. Additional exercise routes require `ASPNETCORE_ENVIRONMENT=Lab`, which the Docker setup sets. Do not forward or publicly host this application.

## Run without Docker

Install .NET SDK 10 and a local SQL Server. Configure `ConnectionStrings__DefaultConnection` as an environment variable (or user secret) pointing at a disposable test database, set `ASPNETCORE_ENVIRONMENT=Lab` if you want to use the additional exercise routes, then run:

```sh
dotnet restore
dotnet run --project vulnerablewebpage.csproj
```

The application creates the schema in the configured database at startup. Do not point it to an existing production database.

## Browser regression tests

Requires Node.js 22+, Chromium and a running lab instance:

```sh
npm install
npx playwright install chromium
npm run test:browser
```

Playwright checks reflected, stored and DOM behaviors, the strict CSP response, IDOR/BOLA ownership checks, parameterized SQL, and the simulated SSRF fixture. GitHub Actions runs the suite in disposable containers. See [security notes](docs/XSS-SECURITY-NOTES.md) for limitations.

## Key files

- `Controllers/XssLabController.cs`: input collection and stored records
- `Views/XssLab/Index.cshtml`: side-by-side rendering cases
- `wwwroot/js/xss-lab.js`: browser DOM demonstration
- `Program.cs`: database startup and CSP switch
- `compose.yaml` and `Dockerfile`: local runtime
- `tests/browser.spec.mjs`: browser tests
- `docs/XSS-SECURITY-NOTES.md`: XSS threat model and verification
- `Controllers/LabsController.cs`: additional exercise routes
- `Services/IsolatedTrainingScenarios.cs`: disposable example implementations
- `Views/Labs/`: interactive comparison pages
- `tests/training-labs.spec.mjs`: additional browser assertions
- `docs/ADDITIONAL-LABS.md`: IDOR/BOLA, SQL injection, and SSRF simulation notes

Use only against systems that you own or are authorized to test.
