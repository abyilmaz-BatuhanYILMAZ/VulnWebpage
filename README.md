# VulnWebPage

A small ASP.NET Core MVC lab project that demonstrates three common XSS categories in both unsafe and safer implementations:

- Reflected XSS
- Stored XSS
- DOM-based XSS

The goal of this project is to understand how XSS appears in real application flows, how different rendering patterns affect browser behavior, and how safer output handling changes the result.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server
- Razor Views
- Bootstrap 5

## Project Purpose

This repository was built as a hands-on learning project to compare vulnerable and safer rendering patterns in one simple web application.

Instead of explaining XSS only theoretically, the project shows how the same user-controlled input behaves differently depending on how it is processed and rendered.

## Covered Scenarios

### 1. Reflected XSS
Input is received from the query string and rendered back to the page in the same request.

- Unsafe example: untrusted input is rendered as raw HTML
- Safer example: untrusted input is rendered through Razor’s default encoded output

### 2. Stored XSS
Input is saved in the database and later displayed again.

- Unsafe example: stored content is rendered as raw HTML
- Safer example: stored content is rendered as encoded text

### 3. DOM XSS
Input is handled in the browser by JavaScript.

- Unsafe example: user-controlled data is written with `innerHTML`
- Safer example: user-controlled data is written with `textContent`

## Why the Unsafe Versions Are Vulnerable

The unsafe examples use dangerous rendering patterns that allow the browser to interpret attacker-controlled input as active HTML or JavaScript.

Examples:
- `Html.Raw(...)`
- `innerHTML`

These patterns may lead to script execution when malicious payloads are supplied.

## Why the Safer Versions Do Not Trigger XSS in the Same Way

The safer examples rely on output encoding or safer DOM APIs.

Examples:
- Razor encoded output such as `@Model.Property`
- `textContent` instead of `innerHTML`

These approaches prevent the browser from treating input as executable code.

## Project Structure

- `Controllers/XssLabController.cs`  
  Handles reflected input, stored entry creation, and stored entry deletion.

- `Data/ApplicationDbContext.cs`  
  EF Core database context.

- `Models/XssEntry.cs`  
  Entity used to store XSS test entries.

- `Models/XssLabPageViewModel.cs`  
  View model used by the main lab page.

- `Views/XssLab/Index.cshtml`  
  Main page that displays reflected, stored, and DOM XSS sections.

- `Program.cs`  
  Application startup and service registration.

## How to Run

### 1. Configure the connection string

Edit `appsettings.json` and set your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VulnerableWebPageDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
