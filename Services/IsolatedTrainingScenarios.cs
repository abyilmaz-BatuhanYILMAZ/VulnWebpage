using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using vulnerablewebpage.Models;

namespace vulnerablewebpage.Services;

/// <summary>
/// Disposable fixtures for three local training exercises. These examples are
/// deliberately vulnerable; do not reuse the unsafe branches in real services.
/// </summary>
public static class IsolatedTrainingScenarios
{
    private static readonly LabDocument[] Documents =
    [
        new(101, "alice", "Alice's project notes", "ALICE_DEMO_DOCUMENT"),
        new(202, "bob", "Bob's private report", "BOB_PRIVATE_DEMO_DOCUMENT"),
        new(103, "alice", "Alice's checklist", "ALICE_SECOND_DOCUMENT")
    ];

    public static AuthorizationLabViewModel ReadDocument(string? actor, int id, bool secure)
    {
        // The actor is chosen by the visitor for demonstration purposes. It is
        // NOT an authenticated or cryptographically verified application identity.
        var selected = actor == "bob" ? "bob" : "alice";
        var document = Documents.FirstOrDefault(item => item.Id == id);
        if (document is null)
            return new() { Actor = selected, DocumentId = id, Secure = secure,
                Outcome = "Document not found." };

        if (secure && document.Owner != selected)
            return new() { Actor = selected, DocumentId = id, Secure = true,
                Outcome = "Access denied: object ownership does not match the selected lab actor." };

        return new() { Actor = selected, DocumentId = id, Secure = secure,
            Document = document, Outcome = "Document returned." };
    }

    public static SqlInjectionLabViewModel SearchInventory(string? input, bool secure)
    {
        var term = input ?? string.Empty;

        // The vulnerable query is deliberately limited to short, read-only
        // boolean-injection exercises against a fresh in-memory database.
        // Disallow separators, filesystem paths, and other SQLite statements.
        if (!Regex.IsMatch(term, @"\A[a-zA-Z0-9 _'=%-]{0,60}\z"))
            return new() { Term = term, Secure = secure,
                Outcome = "Input rejected by the local exercise character and length limits." };

        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using (var seed = connection.CreateCommand())
        {
            seed.CommandText = """
                CREATE TABLE Inventory (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL);
                INSERT INTO Inventory (Id, Name) VALUES (1, 'Notebook');
                INSERT INTO Inventory (Id, Name) VALUES (2, 'Keyboard');
                INSERT INTO Inventory (Id, Name) VALUES (3, 'Router');
                INSERT INTO Inventory (Id, Name) VALUES (4, 'Monitor');
                """;
            seed.ExecuteNonQuery();
        }
        using (var readOnly = connection.CreateCommand())
        {
            readOnly.CommandText = "PRAGMA query_only=ON;";
            readOnly.ExecuteNonQuery();
        }

        using var query = connection.CreateCommand();
        if (secure)
        {
            query.CommandText = "SELECT Id, Name FROM Inventory WHERE Name LIKE @pattern ORDER BY Id";
            query.Parameters.AddWithValue("@pattern", "%" + term + "%");
        }
        else
        {
            // For a boolean injection demonstration, try: ' OR 1=1--
            query.CommandText = "SELECT Id, Name FROM Inventory WHERE Name LIKE '%" +
                term + "%' ORDER BY Id";
        }

        try
        {
            var found = new List<LabInventoryItem>();
            using var reader = query.ExecuteReader();
            while (reader.Read())
                found.Add(new LabInventoryItem(reader.GetInt32(0), reader.GetString(1)));

            return new() { Term = term, Secure = secure, Items = found,
                Outcome = $"Matched {found.Count} inventory records." };
        }
        catch (SqliteException)
        {
            return new() { Term = term, Secure = secure,
                Outcome = "The query failed in the disposable local database." };
        }
    }

    public static SsrfLabViewModel FetchFixture(string? input, bool secure)
    {
        var url = input ?? string.Empty;
        if (url.Length > 200 ||
            !Uri.TryCreate(url, UriKind.Absolute, out var target) ||
            (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(target.UserInfo))
            return new() { Url = url, Secure = secure, Outcome = "Invalid training URL." };

        const string publicAddress = "https://public.lab.invalid/catalog";
        const string privateAddress = "http://internal.lab.invalid/admin";

        if (secure && !string.Equals(target.AbsoluteUri, publicAddress, StringComparison.Ordinal))
            return new() { Url = url, Secure = true, Outcome = "Blocked: destination is not on the exact HTTPS allowlist." };

        // In-memory response fixtures only. There is no DNS resolution, HTTP
        // client, socket, proxy, redirect handling, or outbound network request.
        var response = target.AbsoluteUri switch
        {
            publicAddress => "PUBLIC_CATALOG_DEMO",
            privateAddress => "SIMULATED_INTERNAL_ADMIN_DEMO",
            _ => null
        };
        return new() { Url = url, Secure = secure, ResponseBody = response,
            Outcome = response is null ? "No fixture exists for this URL. No network request was made." :
                "The local fixture returned a response. No network request was made." };
    }
}
