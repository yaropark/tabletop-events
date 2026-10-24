using Dapper;
using Microsoft.Data.Sqlite;

namespace TabletopEvents.Api.Data;

public sealed class Database
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

    private readonly string _connectionString;

    public Database(IConfiguration configuration)
    {
        var builder = new SqliteConnectionStringBuilder(
            configuration.GetConnectionString("Default") ?? "Data Source=data/tabletop-events.db")
        {
            ForeignKeys = true,
            // Concurrent writers wait for the lock instead of failing immediately.
            DefaultTimeout = 30,
        };
        _connectionString = builder.ToString();

        var directory = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        using var connection = Open();
        connection.Execute("PRAGMA journal_mode = WAL;");
        connection.Execute(Schema);
    }

    public static string FormatUtc(DateTime utc) => utc.ToUniversalTime().ToString("O");

    public static DateTime ParseUtc(string value) =>
        DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
}
