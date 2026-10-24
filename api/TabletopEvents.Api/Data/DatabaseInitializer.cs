using Dapper;

namespace TabletopEvents.Api.Data;

/// <summary>Creates the schema on startup. Safe to run on an existing database.</summary>
public static class DatabaseInitializer
{
    private const string Schema = """
        CREATE TABLE IF NOT EXISTS GameTemplates (
            Key                    TEXT    PRIMARY KEY,
            Name                   TEXT    NOT NULL,
            Category               TEXT    NOT NULL,
            Color                  TEXT    NOT NULL,
            FormatsJson            TEXT    NOT NULL,
            DefaultDurationMinutes INTEGER NOT NULL CHECK (DefaultDurationMinutes > 0),
            DefaultCapacity        INTEGER NOT NULL,
            MaxCapacity            INTEGER NOT NULL CHECK (MaxCapacity BETWEEN 1 AND 30),
            MinPlayers             INTEGER NOT NULL CHECK (MinPlayers >= 1)
        );

        -- Capacity, duration, format and min players are copied from the template when
        -- the event is created, so later template edits never change existing events.
        CREATE TABLE IF NOT EXISTS Events (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            TemplateKey     TEXT    NOT NULL REFERENCES GameTemplates(Key),
            Name            TEXT    NOT NULL,
            Format          TEXT    NOT NULL,
            StartUtc        TEXT    NOT NULL,
            DurationMinutes INTEGER NOT NULL CHECK (DurationMinutes > 0),
            Capacity        INTEGER NOT NULL CHECK (Capacity BETWEEN 1 AND 30),
            MinPlayers      INTEGER NOT NULL CHECK (MinPlayers >= 1),
            Location        TEXT    NOT NULL,
            CreatedUtc      TEXT    NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_Events_StartUtc ON Events(StartUtc);

        CREATE TABLE IF NOT EXISTS Registrations (
            Id              INTEGER PRIMARY KEY AUTOINCREMENT,
            EventId         INTEGER NOT NULL REFERENCES Events(Id),
            Name            TEXT    NOT NULL,
            NormalizedName  TEXT    NOT NULL,
            Email           TEXT    NULL,
            NormalizedEmail TEXT    NULL,
            CreatedUtc      TEXT    NOT NULL
        );
        -- Duplicate rule: the email identifies a player when given, otherwise the name does.
        CREATE UNIQUE INDEX IF NOT EXISTS UX_Registrations_Email
            ON Registrations(EventId, NormalizedEmail) WHERE NormalizedEmail IS NOT NULL;
        CREATE UNIQUE INDEX IF NOT EXISTS UX_Registrations_Name
            ON Registrations(EventId, NormalizedName) WHERE NormalizedEmail IS NULL;
        CREATE INDEX IF NOT EXISTS IX_Registrations_EventId ON Registrations(EventId);
        """;

    public static void Initialize(DbConnectionFactory db)
    {
        using var connection = db.Open();
        connection.Execute("PRAGMA journal_mode = WAL;");
        connection.Execute(Schema);
    }
}
