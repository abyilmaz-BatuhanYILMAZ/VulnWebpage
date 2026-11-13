using System.Text.RegularExpressions;
using vulnerablewebpage.Models;

namespace vulnerablewebpage.Services;

/// <summary>
/// Additional isolated exercises. Both use synthetic data only.
/// Neither scenario touches the filesystem, real identities, or a database.
/// </summary>
public static class AdditionalTrainingScenarios
{
    private static readonly IReadOnlyDictionary<string, string> Files =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["/public/readme.txt"] = "PUBLIC_README_FIXTURE",
            ["/public/guide.txt"] = "PUBLIC_TRAINING_GUIDE",
            ["/internal/config.txt"] = "SIMULATED_PRIVATE_CONFIGURATION"
        };

    public static PathTraversalLabViewModel ReadVirtualFile(string? requested, bool secure)
    {
        var path = requested ?? string.Empty;
        if (!Regex.IsMatch(path, @"\A[a-zA-Z0-9_./-]{1,100}\z"))
            return new() { RequestedPath = path, Secure = secure,
                Outcome = "Invalid training path. Only short ASCII relative paths are accepted." };

        // Canonicalize a purely virtual path by interpreting dot segments.
        // This function never accesses a real file or directory.
        var segments = new List<string>();
        foreach (var segment in ("public/" + path).Split('/'))
        {
            if (segment.Length == 0 || segment == ".") continue;
            if (segment == "..")
            {
                if (segments.Count > 0) segments.RemoveAt(segments.Count - 1);
                continue;
            }
            segments.Add(segment);
        }

        var normalized = "/" + string.Join("/", segments);
        if (secure && !normalized.StartsWith("/public/", StringComparison.Ordinal))
            return new() { RequestedPath = path, Secure = true, ResolvedPath = normalized,
                Outcome = "Blocked: canonical path escapes the public document root." };

        Files.TryGetValue(normalized, out var contents);
        return new()
        {
            RequestedPath = path,
            Secure = secure,
            ResolvedPath = normalized,
            Contents = contents,
            Outcome = contents is null ? "Virtual file not found." : "Virtual fixture returned."
        };
    }

    public static MassAssignmentLabViewModel UpdateSyntheticProfile(
        string? requestedName, bool requestedAdmin, bool secure)
    {
        var name = (requestedName ?? string.Empty).Trim();
        if (name.Length is < 1 or > 40)
            return new() { DisplayName = name, RequestedAdmin = requestedAdmin, Secure = secure,
                Submitted = true, Outcome = "Display name must contain 1–40 characters." };

        // Unsafe: the extra client-controlled admin flag is copied into the
        // synthetic profile. Safe: only the intended DisplayName field is accepted.
        var admin = !secure && requestedAdmin;
        return new()
        {
            DisplayName = name,
            RequestedAdmin = requestedAdmin,
            Secure = secure,
            Submitted = true,
            ResultAdmin = admin,
            Outcome = admin
                ? "The untrusted IsAdmin field changed the synthetic role."
                : secure && requestedAdmin
                    ? "The extra IsAdmin field was ignored by the allowlist."
                    : "Profile updated within this request only."
        };
    }
}
