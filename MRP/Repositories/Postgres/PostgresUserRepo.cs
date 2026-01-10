using Npgsql;

namespace FHTW.Swen1.Forum.System;

public sealed class PostgresUserRepo : IUserRepository
{
    public void Add(User user)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
        INSERT INTO users (username, full_name, email, password_hash)
        VALUES (@u, @f, @e, @p);", conn);

        cmd.Parameters.AddWithValue("u", user.UserName);
        cmd.Parameters.AddWithValue("f", user.FullName ?? string.Empty);
        cmd.Parameters.AddWithValue("e", user.EMail ?? string.Empty);
        cmd.Parameters.AddWithValue("p", user.PasswordHash);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // unique_violation
        {
            throw new InvalidOperationException("User already exists.");
        }
    }


    public User? Get(string userName)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            SELECT username, full_name, email, password_hash
            FROM users
            WHERE username = @u;", conn);

        cmd.Parameters.AddWithValue("u", userName);

        using var r = cmd.ExecuteReader();
        if (!r.Read())
            return null;

        var user = new User();
        user.UserName = r.GetString(0);
        user.FullName = r.GetString(1);
        user.EMail = r.GetString(2);
        user.PasswordHash = r.GetString(3);

        return user;
    }

    public bool Update(User user)
    {
        using var conn = DbConnection.CreateAndOpen();

        var setPwd = string.IsNullOrWhiteSpace(user.PasswordHash)
            ? ""
            : ", password_hash = @p";

        using var cmd = new NpgsqlCommand($@"
        UPDATE users
        SET full_name = @f,
            email = @e
            {setPwd}
        WHERE username = @u;", conn);

        cmd.Parameters.AddWithValue("u", user.UserName);
        cmd.Parameters.AddWithValue("f", user.FullName ?? string.Empty);
        cmd.Parameters.AddWithValue("e", user.EMail ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            cmd.Parameters.AddWithValue("p", user.PasswordHash);

        return cmd.ExecuteNonQuery() > 0;
    }


    public bool Delete(string userName)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand("DELETE FROM users WHERE username = @u;", conn);
        cmd.Parameters.AddWithValue("u", userName);

        return cmd.ExecuteNonQuery() > 0;
    }
}
