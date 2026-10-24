using System.Net.Mail;
using Dapper;
using Microsoft.Data.Sqlite;
using TabletopEvents.Api.Data;

namespace TabletopEvents.Api.Registrations;

public sealed record RegisterRequest(string? Name, string? Email);

public sealed record RegistrationResponse(int Id, string Name, int SeatsLeft);

public sealed record RegistrantResponse(string Name, DateTime RegisteredUtc);

public enum RegistrationOutcome { Registered, Invalid, EventNotFound, EventFull, RegistrationClosed, AlreadyRegistered }

public sealed record RegistrationResult(
    RegistrationOutcome Outcome,
    RegistrationResponse? Registration = null,
    Dictionary<string, string[]>? Errors = null);

public sealed class RegistrationService(Database db, StoreClock clock)
{
    private const int SqliteConstraintUnique = 2067;

    public RegistrationResult Register(int eventId, RegisterRequest request)
    {
        var name = string.Join(' ', (request.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        var errors = new Dictionary<string, string[]>();
        if (name.Length is 0 or > 100)
            errors["name"] = ["Please enter your name (max 100 characters)."];
        if (email is not null && (email.Length > 254 || !MailAddress.TryCreate(email, out _)))
            errors["email"] = ["That email address doesn't look right."];
        if (errors.Count > 0)
            return new(RegistrationOutcome.Invalid, Errors: errors);

        var normalizedName = name.ToLowerInvariant();
        var normalizedEmail = email?.ToLowerInvariant();
        var now = Database.FormatUtc(clock.UtcNow);

        using var connection = db.Open();
        try
        {
            // The capacity check and the insert are one statement. SQLite takes the write lock
            // before evaluating it, so two requests racing for the last seat are serialized:
            // the second one sees the updated count and inserts nothing.
            var inserted = connection.Execute("""
                INSERT INTO Registrations (EventId, Name, NormalizedName, Email, NormalizedEmail, CreatedUtc)
                SELECT @eventId, @name, @normalizedName, @email, @normalizedEmail, @now
                WHERE (SELECT COUNT(*) FROM Registrations WHERE EventId = @eventId)
                    < (SELECT Capacity FROM Events WHERE Id = @eventId AND StartUtc > @now)
                """,
                new { eventId, name, normalizedName, email, normalizedEmail, now });

            if (inserted == 1)
            {
                var (id, seatsLeft) = connection.QuerySingle<(long, long)>("""
                    SELECT last_insert_rowid(),
                           (SELECT Capacity FROM Events WHERE Id = @eventId)
                         - (SELECT COUNT(*) FROM Registrations WHERE EventId = @eventId)
                    """, new { eventId });
                return new(RegistrationOutcome.Registered, new RegistrationResponse((int)id, name, (int)seatsLeft));
            }
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteConstraintUnique)
        {
            return new(RegistrationOutcome.AlreadyRegistered);
        }

        // Nothing was inserted. The decision is already made; these reads only pick the message.
        var startUtc = connection.QuerySingleOrDefault<string>(
            "SELECT StartUtc FROM Events WHERE Id = @eventId", new { eventId });
        if (startUtc is null)
            return new(RegistrationOutcome.EventNotFound);

        var isDuplicate = connection.ExecuteScalar<bool>("""
            SELECT EXISTS (SELECT 1 FROM Registrations WHERE EventId = @eventId AND
                (CASE WHEN @normalizedEmail IS NULL
                      THEN NormalizedEmail IS NULL AND NormalizedName = @normalizedName
                      ELSE NormalizedEmail = @normalizedEmail END))
            """, new { eventId, normalizedName, normalizedEmail });
        if (isDuplicate)
            return new(RegistrationOutcome.AlreadyRegistered);

        return Database.ParseUtc(startUtc) <= clock.UtcNow
            ? new(RegistrationOutcome.RegistrationClosed)
            : new(RegistrationOutcome.EventFull);
    }

    public IReadOnlyList<RegistrantResponse> List(int eventId)
    {
        using var connection = db.Open();
        return connection
            .Query<(string Name, string CreatedUtc)>(
                "SELECT Name, CreatedUtc FROM Registrations WHERE EventId = @eventId ORDER BY Id", new { eventId })
            .Select(r => new RegistrantResponse(r.Name, Database.ParseUtc(r.CreatedUtc)))
            .ToList();
    }
}
