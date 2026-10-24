using Microsoft.Data.Sqlite;

namespace TabletopEvents.Api.Data;

/// <summary>Opens connections to the SQLite file configured in ConnectionStrings:Default.</summary>
public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration, IHostEnvironment environment)
    {
        var builder = new SqliteConnectionStringBuilder(
            configuration.GetConnectionString("Default") ?? "Data Source=db/tabletop-events.db")
        {
            ForeignKeys = true,
            // Concurrent writers wait for the lock instead of failing immediately.
            DefaultTimeout = 30,
        };
        // Relative paths resolve against the project, not whatever directory `dotnet run` was started from.
        builder.DataSource = Path.GetFullPath(builder.DataSource, environment.ContentRootPath);
        _connectionString = builder.ToString();

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
