using Dapper;
using Microsoft.Data.Sqlite;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Models.Enums;
using TabletopEvents.Api.Repositories.Contracts;

namespace TabletopEvents.Api.Repositories.Implementations;

public sealed class RegistrationRepository(DbConnectionFactory db) : IRegistrationRepository
{
    private const int SqliteConstraintUnique = 2067;

    public (RegistrationInsertStatus Status, int Id) TryInsertWithinCapacity(Registration registration, DateTime nowUtc)
    {
        using var connection = db.Open();
        try
        {
            // The capacity check and the insert are ONE statement. SQLite takes the write lock
            // before evaluating it, so two requests racing for the last seat are serialized:
            // the second one sees the updated count and inserts nothing.
            // Do not split this into "count, then insert" in a service - that reintroduces the race.
            var inserted = connection.Execute("""
                INSERT INTO Registrations (EventId, Name, NormalizedName, Email, NormalizedEmail, CreatedUtc)
                SELECT @EventId, @Name, @NormalizedName, @Email, @NormalizedEmail, @CreatedUtc
                WHERE (SELECT COUNT(*) FROM Registrations WHERE EventId = @EventId)
                    < (SELECT Capacity FROM Events WHERE Id = @EventId AND StartUtc > @Now)
                """,
                new
                {
                    registration.EventId,
                    registration.Name,
                    registration.NormalizedName,
                    registration.Email,
                    registration.NormalizedEmail,
                    CreatedUtc = SqliteDates.Format(registration.CreatedUtc),
                    Now = SqliteDates.Format(nowUtc),
                });

            return inserted == 1
                ? (RegistrationInsertStatus.Inserted, (int)connection.ExecuteScalar<long>("SELECT last_insert_rowid()"))
                : (RegistrationInsertStatus.NotInserted, 0);
        }
        catch (SqliteException ex) when (ex.SqliteExtendedErrorCode == SqliteConstraintUnique)
        {
            return (RegistrationInsertStatus.Duplicate, 0);
        }
    }

    public bool Exists(int eventId, string normalizedName, string? normalizedEmail)
    {
        using var connection = db.Open();
        return connection.ExecuteScalar<bool>("""
            SELECT EXISTS (SELECT 1 FROM Registrations WHERE EventId = @eventId AND
                (CASE WHEN @normalizedEmail IS NULL
                      THEN NormalizedEmail IS NULL AND NormalizedName = @normalizedName
                      ELSE NormalizedEmail = @normalizedEmail END))
            """, new { eventId, normalizedName, normalizedEmail });
    }

    public int Count(int eventId)
    {
        using var connection = db.Open();
        return (int)connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Registrations WHERE EventId = @eventId", new { eventId });
    }

    public IReadOnlyList<Registration> ListByEvent(int eventId)
    {
        using var connection = db.Open();
        return connection
            .Query<RegistrationRow>("""
                SELECT Id, EventId, Name, NormalizedName, Email, NormalizedEmail, CreatedUtc
                FROM Registrations WHERE EventId = @eventId ORDER BY Id
                """, new { eventId })
            .Select(r => new Registration
            {
                Id = (int)r.Id,
                EventId = (int)r.EventId,
                Name = r.Name,
                NormalizedName = r.NormalizedName,
                Email = r.Email,
                NormalizedEmail = r.NormalizedEmail,
                CreatedUtc = SqliteDates.Parse(r.CreatedUtc),
            })
            .ToList();
    }

    private sealed class RegistrationRow
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public string Name { get; set; } = "";
        public string NormalizedName { get; set; } = "";
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public string CreatedUtc { get; set; } = "";
    }
}
