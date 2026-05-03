using BeerJournal.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class BeerRepository : BaseRepository
{
    public BeerRepository(IConfiguration configuration) : base(configuration) { }

    // GET all beers — used to populate the dropdown in log-beer
    public List<Beer> GetAllBeers()
    {
        var beers = new List<Beer>();

        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT beer_id, name, brand, alcohol_pct, category
                       FROM beers
                       ORDER BY name";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            beers.Add(MapBeer(reader));
        }

        return beers;
    }

    // GET one beer by id
    public Beer? GetBeerById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT beer_id, name, brand, alcohol_pct, category
                       FROM beers
                       WHERE beer_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return MapBeer(reader);
        }

        return null;
    }

    // POST — create a new beer in the catalogue
    // Returns the newly created beer (with its generated beer_id)
    public Beer? CreateBeer(Beer beer)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"INSERT INTO beers (name, brand, alcohol_pct, category)
                       VALUES (@name, @brand, @alcoholPct, @category)
                       RETURNING beer_id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name",       beer.Name);
        cmd.Parameters.AddWithValue("@brand",      (object?)beer.Brand ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@alcoholPct", (object?)beer.AlcoholPct ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@category",   (object?)beer.Category ?? DBNull.Value);

        var newId = cmd.ExecuteScalar();
        if (newId == null) return null;

        beer.BeerId = Convert.ToInt32(newId);
        return beer;
    }

    // PUT — update an existing beer
    public bool UpdateBeer(Beer beer)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"UPDATE beers
                       SET name        = @name,
                           brand       = @brand,
                           alcohol_pct = @alcoholPct,
                           category    = @category
                       WHERE beer_id = @beerId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@beerId",     beer.BeerId);
        cmd.Parameters.AddWithValue("@name",       beer.Name);
        cmd.Parameters.AddWithValue("@brand",      (object?)beer.Brand ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@alcoholPct", (object?)beer.AlcoholPct ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@category",   (object?)beer.Category ?? DBNull.Value);

        // ExecuteNonQuery returns the number of affected rows
        // 0 = no beer with that id was found, so we return false
        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE — remove a beer from the catalogue
    // Will fail if any tasting_entries still reference this beer
    // (because of the ON DELETE RESTRICT we set on the foreign key)
    public bool DeleteBeer(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"DELETE FROM beers
                       WHERE beer_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Helper — converts a database row into a Beer object
    private Beer MapBeer(NpgsqlDataReader reader)
    {
        return new Beer
        {
            BeerId     = Convert.ToInt32(reader["beer_id"]),
            Name       = reader["name"]?.ToString() ?? "",
            Brand      = reader["brand"] == DBNull.Value ? null : reader["brand"].ToString(),
            AlcoholPct = reader["alcohol_pct"] == DBNull.Value ? null : Convert.ToDecimal(reader["alcohol_pct"]),
            Category   = reader["category"] == DBNull.Value ? null : reader["category"].ToString()
        };
    }
}