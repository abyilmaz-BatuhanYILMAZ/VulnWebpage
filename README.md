# VulnWebPage — Application Security Training Lab

[![Application security lab regression tests](https://github.com/abyilmaz-BatuhanYILMAZ/VulnWebpage/actions/workflows/verify.yml/badge.svg)](https://github.com/abyilmaz-BatuhanYILMAZ/VulnWebpage/actions/workflows/verify.yml)

**An interactive, deliberately vulnerable ASP.NET Core laboratory for practicing web application security.** Examine insecure behavior, test the same input against a defensive implementation, and verify the difference using browser automation.

The project covers **six learning modules and eight attack scenarios**. It is not limited to XSS.

> **Safety:** This application contains working vulnerable examples. Run it only on your own computer, in the isolated Docker setup, with synthetic data. Never publish it as a public website or point it to a production database.

## What is inside?

| Module | Vulnerable behavior | Defensive concept | Exercise |
| --- | --- | --- | --- |
| **Cross-site scripting** | Reflected, stored and DOM injection | Razor output encoding, `textContent`, CSP | [XSS](Views/XssLab/Index.cshtml) |
| **IDOR / BOLA** | Accessing another actor's synthetic document by ID | Object-ownership authorization | [Authorization](Views/Labs/Authorization.cshtml) |
| **SQL injection** | String-concatenated SQL search | Parameter binding | [SQL injection](Views/Labs/SqlInjection.cshtml) |
| **SSRF** | Accepting a virtual internal destination | Exact destination allowlist | [SSRF](Views/Labs/Ssrf.cshtml) |
| **Path traversal** | Escaping a virtual public document root | Canonical path boundary checking | [Path traversal](Views/Labs/PathTraversal.cshtml) |
| **Mass assignment** | Binding an extra role field from a request | Explicit writable-field allowlist | [Overposting](Views/Labs/MassAssignment.cshtml) |

**Important limitations:** IDOR/BOLA uses simulated actors, not a real login system. SQL injection runs against fresh, read-only SQLite memory. SSRF uses static responses and makes **no network requests**. Path traversal never reads real files. Mass assignment never changes a real account. These safety boundaries are intentional, and the exercises must not be mistaken for production-ready implementations.

## Quick start — Docker

**Requirements:** Docker Engine / Docker Desktop with Compose, a modern browser, and enough RAM for SQL Server.

1. Clone this repository and enter its directory.

   ```sh
   git clone https://github.com/abyilmaz-BatuhanYILMAZ/VulnWebpage.git
   cd VulnWebpage
   ```

2. Copy the example environment file and replace the sample database password with your own unique, strong local password.

   **macOS / Linux:**
   ```sh
   cp .env.example .env
   ```

   **Windows PowerShell:**
   ```powershell
   Copy-Item .env.example .env
   ```

   The `.env` file must stay local; it is covered by `.gitignore`.

3. Start the isolated environment.

   ```sh
   docker compose up --build -d
   ```

4. Open the **[interactive lab overview](http://127.0.0.1:8080/labs/)**.

   - Overview and completion checklist: http://127.0.0.1:8080/labs/
   - XSS and CSP comparison: http://127.0.0.1:8080/?csp=off
   - IDOR / BOLA: http://127.0.0.1:8080/labs/authorization
   - SQL injection: http://127.0.0.1:8080/labs/sql-injection
   - SSRF simulation: http://127.0.0.1:8080/labs/ssrf
   - Path traversal: http://127.0.0.1:8080/labs/path-traversal
   - Mass assignment: http://127.0.0.1:8080/labs/mass-assignment

5. After training, remove the containers **and disposable database volume**.

   ```sh
   docker compose down --volumes
   ```

Docker publishes the application only to `127.0.0.1:8080` on the host. SQL Server port 1433 is not exposed to the host. No production credentials or customer data are needed.

## How to learn with each exercise

1. **Predict:** What should happen if the submitted value is trusted without validation or authorization?
2. **Reproduce:** Run the supplied harmless demonstration input in **unsafe** mode.
3. **Compare:** Repeat the same input with **safe** mode enabled.
4. **Explain:** Name the trust boundary, root cause, fix and remaining limitations.
5. **Verify:** Run the regression tests or write another assertion.

The overview page includes a **manual completion checklist**. Its state is stored in your browser only using `localStorage`. It does not measure scores, identify visitors or create accounts.

## Automated verification

This project uses **Playwright + Chromium** for end-to-end comparisons and **GitHub Actions** to build the local Docker environment and execute the suite on pushes and pull requests.

With Docker running, install Node.js 22+ and run:

```sh
npm install
npx playwright install chromium
npm run test:browser
```

CI results: [Actions / verify.yml](https://github.com/abyilmaz-BatuhanYILMAZ/VulnWebpage/actions/workflows/verify.yml).

A green build verifies the named assertions; it **does not** certify the application as secure or test every possible attack.

## Technical architecture

- **Application:** ASP.NET Core MVC on **.NET 10 LTS**
- **Persistent exercise storage:** SQL Server 2022 Developer, used only for the stored XSS module
- **SQL injection fixture:** SQLite `:memory:`, seeded per request and set to query-only mode
- **Other modules:** In-memory sample documents, virtual files and request-local profiles
- **Browser:** Bootstrap, Razor views and small first-party JavaScript modules
- **Deployment:** Docker Compose, loopback-bound web port; no published SQL Server port
- **Startup guard:** The application refuses to start unless `ASPNETCORE_ENVIRONMENT=Lab`
- **CSP:** Opt-in on the original XSS exercise; enforced by default on the additional lab pages

### Relevant source paths

| Path | Purpose |
| --- | --- |
| [Controllers/XssLabController.cs](Controllers/XssLabController.cs) | Original XSS training controller |
| [Controllers/LabsController.cs](Controllers/LabsController.cs) | Other training routes |
| [Services/IsolatedTrainingScenarios.cs](Services/IsolatedTrainingScenarios.cs) | IDOR, SQL injection and SSRF examples |
| [Services/AdditionalTrainingScenarios.cs](Services/AdditionalTrainingScenarios.cs) | Path traversal and mass assignment examples |
| [Views/Labs/](Views/Labs/) | Training hub and exercise views |
| [wwwroot/js/lab-progress.js](wwwroot/js/lab-progress.js) | Browser-local manual progress checklist |
| [tests/](tests/) | Automated browser regression tests |
| [docs/XSS-SECURITY-NOTES.md](docs/XSS-SECURITY-NOTES.md) | XSS analysis |
| [docs/ADDITIONAL-LABS.md](docs/ADDITIONAL-LABS.md) | IDOR, SQL injection and SSRF analysis |
| [docs/EXTENDED-LABS.md](docs/EXTENDED-LABS.md) | Path traversal, overposting and local progress design |

## Local execution without Docker

Install **.NET SDK 10** and a disposable local SQL Server. Provide `ConnectionStrings__DefaultConnection` using a user secret or environment variable. Explicitly set `ASPNETCORE_ENVIRONMENT=Lab`; the application deliberately fails at startup in any other environment.

```sh
dotnet restore
dotnet run --project vulnerablewebpage.csproj
```

Never configure a reachable public listener for this project.

## Scope and responsible use

VulnWebPage is a training project, not a vulnerability scanner or a production security framework. Its unsafe paths are intentional, all fixtures are synthetic, and several demonstrations are simulations. Use it only in environments you own or have explicit authorization to test. Never copy the unsafe implementations into real applications.

