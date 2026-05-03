using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class BaseRepository
{
    protected string ConnectionString { get; }

    public BaseRepository(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("AppProgDb") ?? "";
    }

    protected NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}