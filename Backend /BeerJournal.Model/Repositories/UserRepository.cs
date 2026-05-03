using BeerJournal.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class UserRepository : BaseRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public List<User> GetAllUsers()
    {
        var users = new List<User>();

        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT user_id, first_name, last_name, email, zip_code, city 
                       FROM users
                       ORDER BY user_id";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new User
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName = reader["last_name"]?.ToString() ?? "",
                Email = reader["email"]?.ToString() ?? "",
                ZipCode = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"]),
                City = reader["city"]?.ToString()
            });
        }

        return users;
    }

    public User? GetUserById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT user_id, first_name, last_name, email, zip_code, city 
                       FROM users
                       WHERE user_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName = reader["last_name"]?.ToString() ?? "",
                Email = reader["email"]?.ToString() ?? "",
                ZipCode = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"]),
                City = reader["city"]?.ToString()
            };
        }

        return null;
    }

 // Check if an email is already registered
    // Used in register to prevent duplicate accounts
    public User? GetUserByEmail(string email)
    {
        using var conn = GetConnection();
        conn.Open();
 
        string sql = @"SELECT user_id, first_name, last_name, email, zip_code, city 
                       FROM users
                       WHERE email = @email 
                       LIMIT 1";
 
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email", email);
 
        using var reader = cmd.ExecuteReader();
 
        if (reader.Read())
        {
            return new User
            {
                UserId    = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName  = reader["last_name"]?.ToString() ?? "",
                Email     = reader["email"]?.ToString() ?? "",
                ZipCode   = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"]),
                City      = reader["city"]?.ToString()
            };
        }
 
        return null;
    }





    // Register a new user
    // Password is hashed with bcrypt via pgcrypto — never stored as plain text
    public bool RegisterUser(User user, string password)
    {
        using var conn = GetConnection();
        conn.Open();
 
        string sql = @"INSERT INTO users (first_name, last_name, email, password_hash, zip_code, city)
                       VALUES (@firstName, @lastName, @email, crypt(@password, gen_salt('bf')), @zipCode, @city)";
 
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@lastName",  user.LastName);
        cmd.Parameters.AddWithValue("@email",     user.Email);
        cmd.Parameters.AddWithValue("@password",  password);
        cmd.Parameters.AddWithValue("@zipCode",   (object?)user.ZipCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@city",      (object?)user.City ?? DBNull.Value);
 
        return cmd.ExecuteNonQuery() > 0;
    }
 
    // Validate login — checks email + password against the hashed value in the db
    // crypt(@password, password_hash) re-hashes using the stored salt and compares
    // Returns the user if valid, null if not
    public User? ValidateLogin(string email, string password)
    {
        using var conn = GetConnection();
        conn.Open();
 
        string sql = @"SELECT user_id, first_name, last_name, email, zip_code, city
                       FROM users
                       WHERE email = @email 
                       AND password_hash = crypt(@password, password_hash)";
 
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@email",    email);
        cmd.Parameters.AddWithValue("@password", password);
 
        using var reader = cmd.ExecuteReader();
 
        if (reader.Read())
        {
            return new User
            {
                UserId    = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName  = reader["last_name"]?.ToString() ?? "",
                Email     = reader["email"]?.ToString() ?? "",
                ZipCode   = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"]),
                City      = reader["city"]?.ToString()
            };
        }
 
        // Returns null if email not found or password wrong
        return null;
    }
 
    // Original CreateUser kept for backwards compatibility
    public bool CreateUser(User user)
    {
        using var conn = GetConnection();
        conn.Open();
 
        string sql = @"INSERT INTO users (first_name, last_name, email, zip_code, city)
                       VALUES (@firstName, @lastName, @email, @zipCode, @city)";
 
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@lastName",  user.LastName);
        cmd.Parameters.AddWithValue("@email",     user.Email);
        cmd.Parameters.AddWithValue("@zipCode",   (object?)user.ZipCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@city",      (object?)user.City ?? DBNull.Value);
 
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool UpdateUser(User user)
{
    using var conn = GetConnection();
    conn.Open();

    string sql = @"UPDATE users
                   SET first_name = @firstName,
                       last_name = @lastName,
                       email = @email
                   WHERE user_id = @userId";

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@userId", user.UserId);
    cmd.Parameters.AddWithValue("@firstName", user.FirstName);
    cmd.Parameters.AddWithValue("@lastName", user.LastName);
    cmd.Parameters.AddWithValue("@email", user.Email);

    return cmd.ExecuteNonQuery() > 0;
}

public bool DeleteUser(int userId)
{
    using var conn = GetConnection();
    conn.Open();

    string sql = @"DELETE FROM users
                   WHERE user_id = @userId";

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@userId", userId);

    return cmd.ExecuteNonQuery() > 0;
}
}

