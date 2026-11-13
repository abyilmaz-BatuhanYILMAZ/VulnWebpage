namespace vulnerablewebpage.Models;

public sealed record LabDocument(int Id, string Owner, string Title, string Body);
public sealed record LabInventoryItem(int Id, string Name);

public sealed class AuthorizationLabViewModel
{
    public string Actor { get; init; } = "alice";
    public int DocumentId { get; init; } = 202;
    public bool Secure { get; init; }
    public LabDocument? Document { get; init; }
    public string Outcome { get; init; } = string.Empty;
}

public sealed class SqlInjectionLabViewModel
{
    public string Term { get; init; } = string.Empty;
    public bool Secure { get; init; }
    public IReadOnlyList<LabInventoryItem> Items { get; init; } = [];
    public string Outcome { get; init; } = string.Empty;
}

public sealed class SsrfLabViewModel
{
    public string Url { get; init; } = string.Empty;
    public bool Secure { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? ResponseBody { get; init; }
}
