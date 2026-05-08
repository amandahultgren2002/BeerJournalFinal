// TastingEntryRepository — handles all database operations for tasting entries

using BeerJournal.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class TastingEntryRepository : BaseRepository
{
    // Constructor — passes config to BaseRepository so we get the connection string
    public TastingEntryRepository(IConfiguration configuration) : base(configuration) { }

    // GET — fetch all tasting entries for a specific user
    // Includes beer data by joining with the beers table
    public List<TastingEntry> GetEntriesByUser(int userId)
    {
        var entries = new List<TastingEntry>();

        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT te.entry_id, te.user_id, te.beer_id,
                              te.rating, te.location, te.testing_date,
                              te.price, te.notes, te.latitude, te.longitude,
                              b.name AS beer_name, b.brand, b.alcohol_pct, b.category
                       FROM tasting_entries te
                       INNER JOIN beers b ON te.beer_id = b.beer_id
                       WHERE te.user_id = @userId
                       ORDER BY te.entry_id DESC";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();

        // Loop through each row and build a TastingEntry object
        while (reader.Read())
        {
            entries.Add(MapEntry(reader));
        }

        return entries;
    }

    // GET — fetch one tasting entry by id
    // Also includes beer data via join
    public TastingEntry? GetEntryById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT te.entry_id, te.user_id, te.beer_id,
                              te.rating, te.location, te.testing_date,
                              te.price, te.notes, te.latitude, te.longitude,
                              b.name AS beer_name, b.brand, b.alcohol_pct, b.category
                       FROM tasting_entries te
                       INNER JOIN beers b ON te.beer_id = b.beer_id
                       WHERE te.entry_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();

        // If a row is found, map it to an object
        if (reader.Read())
        {
            return MapEntry(reader);
        }

        return null;
    }

    // POST — create a new tasting entry
    // Only stores beer_id (beer details come from the beers table)
    public bool CreateEntry(TastingEntry entry)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"INSERT INTO tasting_entries
                         (user_id, beer_id, rating, location, testing_date,
                          price, notes, latitude, longitude)
                       VALUES
                         (@userId, @beerId, @rating, @location, @testingDate,
                          @price, @notes, @latitude, @longitude)";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId",      entry.UserId);
        cmd.Parameters.AddWithValue("@beerId",      entry.BeerId);
        cmd.Parameters.AddWithValue("@rating",      (object?)entry.Rating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@location",    (object?)entry.Location ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@testingDate", (object?)entry.TestingDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@price",       (object?)entry.Price ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@notes",       (object?)entry.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@latitude",    (object?)entry.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@longitude",   (object?)entry.Longitude ?? DBNull.Value);

        // ExecuteNonQuery returns number of inserted rows (should be 1 if successful)
        return cmd.ExecuteNonQuery() > 0;
    }

    // PUT — update an existing tasting entry
    public bool UpdateEntry(TastingEntry entry)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"UPDATE tasting_entries
                       SET beer_id      = @beerId,
                           rating       = @rating,
                           location     = @location,
                           testing_date = @testingDate,
                           price        = @price,
                           notes        = @notes,
                           latitude     = @latitude,
                           longitude    = @longitude
                       WHERE entry_id = @entryId";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@entryId",     entry.EntryId);
        cmd.Parameters.AddWithValue("@beerId",      entry.BeerId);
        cmd.Parameters.AddWithValue("@rating",      (object?)entry.Rating ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@location",    (object?)entry.Location ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@testingDate", (object?)entry.TestingDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@price",       (object?)entry.Price ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@notes",       (object?)entry.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@latitude",    (object?)entry.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@longitude",   (object?)entry.Longitude ?? DBNull.Value);

        // Returns number of affected rows (0 = entry not found)
        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE — remove a tasting entry by id
    public bool DeleteEntry(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"DELETE FROM tasting_entries
                       WHERE entry_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Helper — converts a database row into a TastingEntry object
    // Includes nested Beer object from joined columns
    // Used to avoid duplicating mapping logic in multiple methods
    private TastingEntry MapEntry(NpgsqlDataReader reader)
    {
        return new TastingEntry
        {
            EntryId     = Convert.ToInt32(reader["entry_id"]),
            UserId      = Convert.ToInt32(reader["user_id"]),
            BeerId      = Convert.ToInt32(reader["beer_id"]),
            Rating      = reader["rating"] == DBNull.Value ? null : Convert.ToInt32(reader["rating"]),
            Location    = reader["location"] == DBNull.Value ? null : reader["location"].ToString(),
            TestingDate = reader["testing_date"] == DBNull.Value ? null : (DateOnly?)reader["testing_date"],
            Price       = reader["price"] == DBNull.Value ? null : Convert.ToDecimal(reader["price"]),
            Notes       = reader["notes"] == DBNull.Value ? null : reader["notes"].ToString(),
            Latitude    = reader["latitude"] == DBNull.Value ? null : Convert.ToDouble(reader["latitude"]),
            Longitude   = reader["longitude"] == DBNull.Value ? null : Convert.ToDouble(reader["longitude"]),

            // Build the nested Beer object using the joined columns
            Beer = new Beer
            {
                BeerId     = Convert.ToInt32(reader["beer_id"]),
                Name       = reader["beer_name"]?.ToString() ?? "",
                Brand      = reader["brand"] == DBNull.Value ? null : reader["brand"].ToString(),
                AlcoholPct = reader["alcohol_pct"] == DBNull.Value ? null : Convert.ToDecimal(reader["alcohol_pct"]),
                Category   = reader["category"] == DBNull.Value ? null : reader["category"].ToString()
            }
        };
    }
}