using Microsoft.AspNetCore.Mvc;
using vulnerablewebpage.Services;

namespace vulnerablewebpage.Controllers;

[Route("labs")]
public sealed class LabsController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("authorization")]
    public IActionResult Authorization(string? actor = "alice", int id = 202, string? mode = "unsafe")
    {
        var secure = string.Equals(mode, "safe", StringComparison.OrdinalIgnoreCase);
        return View(IsolatedTrainingScenarios.ReadDocument(actor, id, secure));
    }

    [HttpGet("sql-injection")]
    public IActionResult SqlInjection(string? term = "book", string? mode = "unsafe")
    {
        var secure = string.Equals(mode, "safe", StringComparison.OrdinalIgnoreCase);
        return View(IsolatedTrainingScenarios.SearchInventory(term, secure));
    }

    [HttpGet("ssrf")]
    public IActionResult Ssrf(string? url = "http://internal.lab.invalid/admin", string? mode = "unsafe")
    {
        var secure = string.Equals(mode, "safe", StringComparison.OrdinalIgnoreCase);
        return View(IsolatedTrainingScenarios.FetchFixture(url, secure));
    }
}
