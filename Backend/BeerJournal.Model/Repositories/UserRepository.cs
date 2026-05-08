// UserRepository — handles all database operations for users

using BeerJournal.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BeerJournal.Model.Repositories;

public class UserRepository : BaseRepository
{
    // Constructor — passes config to BaseRepository so we get the connection string
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    // GET — fetch all users from the database
    public List<User> GetAllUsers()
    {
        var users = new List<User>();

        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT user_id, first_name, last_name, email, zip_code 
                       FROM users
                       ORDER BY user_id";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        // Loop through each row and build a User object
        while (reader.Read())
        {
            users.Add(new User
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName = reader["last_name"]?.ToString() ?? "",
                Email = reader["email"]?.ToString() ?? "",
                ZipCode = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"])
            });
        }

        return users;
    }

    // GET — fetch one user by id
    public User? GetUserById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT user_id, first_name, last_name, email, zip_code 
                       FROM users
                       WHERE user_id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();

        // If a row is found, map it to a User object
        if (reader.Read())
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                FirstName = reader["first_name"]?.ToString() ?? "",
                LastName = reader["last_name"]?.ToString() ?? "",
                Email = reader["email"]?.ToString() ?? "",
                ZipCode = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"])
            };
        }

        return null;
    }

    // GET — fetch one user by email
    // Used during registration to check if the email already exists
    public User? GetUserByEmail(string email)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"SELECT user_id, first_name, last_name, email, zip_code 
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
                ZipCode   = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"])
            };
        }

        return null;
    }

    // POST — register a new user
    // Password is hashed using bcrypt before being stored
    public bool RegisterUser(User user, string password)
    {
        using var conn = GetConnection();
        conn.Open();

        // crypt(@password, gen_salt('bf')) hashes the password using bcrypt
        string sql = @"INSERT INTO users (first_name, last_name, email, password_hash, zip_code)
                       VALUES (@firstName, @lastName, @email, crypt(@password, gen_salt('bf')), @zipCode)";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@lastName",  user.LastName);
        cmd.Parameters.AddWithValue("@email",     user.Email);
        cmd.Parameters.AddWithValue("@password",  password);
        cmd.Parameters.AddWithValue("@zipCode",   (object?)user.ZipCode ?? DBNull.Value);

        // Returns number of inserted rows (should be 1 if successful)
        return cmd.ExecuteNonQuery() > 0;
    }

    // GET — validate login credentials
    // Returns the user if email + password match, otherwise null
    public User? ValidateLogin(string email, string password)
    {
        using var conn = GetConnection();
        conn.Open();

        // crypt(@password, password_hash) re-hashes the input with the stored salt
        // and compares it to the stored hash — match means correct password
        string sql = @"SELECT user_id, first_name, last_name, email, zip_code
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
                ZipCode   = reader["zip_code"] == DBNull.Value ? null : Convert.ToInt32(reader["zip_code"])
            };
        }

        // Email not found or password incorrect
        return null;
    }

    // POST — create a user without password
    // Kept for backwards compatibility (older parts of the app may still use it)
    public bool CreateUser(User user)
    {
        using var conn = GetConnection();
        conn.Open();

        string sql = @"INSERT INTO users (first_name, last_name, email, zip_code)
                       VALUES (@firstName, @lastName, @email, @zipCode)";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@lastName",  user.LastName);
        cmd.Parameters.AddWithValue("@email",     user.Email);
        cmd.Parameters.AddWithValue("@zipCode",   (object?)user.ZipCode ?? DBNull.Value);

        return cmd.ExecuteNonQuery() > 0;
    }

    // PUT — update an existing user's basic info
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

        // Returns number of affected rows (0 = user not found)
        return cmd.ExecuteNonQuery() > 0;
    }

    // DELETE — remove a user by id
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