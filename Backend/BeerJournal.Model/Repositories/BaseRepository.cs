// BaseRepository — shared parent class for all repositories
// Holds the database connection string and a helper to open new connections
// All other repositories (User, Beer, TastingEntry) inherit from this

using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class BaseRepository
{
    // The connection string from appsettings.json (tells us how to reach the database)
    protected string ConnectionString { get; }

    // Constructor — ASP.NET passes in the app's configuration automatically
    public BaseRepository(IConfiguration configuration)
    {
        // Read the "AppProgDb" entry from appsettings.json's ConnectionStrings section
        ConnectionString = configuration.GetConnectionString("AppProgDb") ?? "";
    }

    // Helper — creates a new database connection
    // Used by every method in the child repositories
    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}