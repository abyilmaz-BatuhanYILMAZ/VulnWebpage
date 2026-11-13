namespace vulnerablewebpage.Models;

public sealed record VirtualFile(string Path, string Contents);

public sealed class PathTraversalLabViewModel
{
    public string RequestedPath { get; init; } = "../internal/config.txt";
    public bool Secure { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? ResolvedPath { get; init; }
    public string? Contents { get; init; }
}

public sealed class MassAssignmentLabViewModel
{
    public string DisplayName { get; init; } = "Student";
    public bool RequestedAdmin { get; init; }
    public bool Secure { get; init; }
    public bool Submitted { get; init; }
    public bool ResultAdmin { get; init; }
    public string Outcome { get; init; } = string.Empty;
}
