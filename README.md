# VulnWebPage — XSS training lab

ASP.NET Core MVC (.NET 10 LTS) examples comparing vulnerable and safer output rendering. This project is **intentionally vulnerable** and intended only for local, isolated security education.

## Exercises

| Type | Unsafe output | Safer output |
| --- | --- | --- |
| Reflected XSS | `Html.Raw(Model.ReflectedInput)` | Razor-encoded text |
| Stored XSS | `Html.Raw(item.Content)` | Razor-encoded text |
| DOM XSS | `innerHTML` | `textContent` |
| Defense in depth | `/?csp=off` | `/?csp=strict` |

CSP enforcement blocks inline JavaScript in the example but **does not fix unsafe rendering**.

## Docker setup

Requires Docker Compose, a modern browser and enough available memory for SQL Server.

1. Copy `.env.example` to `.env` and replace the sample password with a strong, unique local value.
2. Start:

```sh
docker compose up --build -d
```

3. Open [the local lab](http://127.0.0.1:8080/?csp=off), then select **CSP Enforced** to compare outcomes.
4. Stop and remove the disposable database:

```sh
docker compose down --volumes
```

The website is published only on the host loopback interface, while SQL Server remains internal to the Docker network. Do not forward or publicly host this application.

## Run without Docker

Install .NET SDK 10 and a local SQL Server. Configure `ConnectionStrings__DefaultConnection` as an environment variable (or user secret) pointing at a disposable test database; then run:

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

Playwright checks reflected, stored and DOM behaviors and the strict CSP response. GitHub Actions runs the suite in disposable containers. See [security notes](docs/XSS-SECURITY-NOTES.md) for limitations.

## Key files

- `Controllers/XssLabController.cs`: input collection and stored records
- `Views/XssLab/Index.cshtml`: side-by-side rendering cases
- `wwwroot/js/xss-lab.js`: browser DOM demonstration
- `Program.cs`: database startup and CSP switch
- `compose.yaml` and `Dockerfile`: local runtime
- `tests/browser.spec.mjs`: browser tests
- `docs/XSS-SECURITY-NOTES.md`: threat model and verification

Use only against systems that you own or are authorized to test.
