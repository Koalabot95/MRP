using Npgsql;
using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System;

public sealed class PostgresRatingRepo : IRatingRepository
{
    public void Add(Rating rating)
    {
        using var conn = DbConnection.CreateAndOpen();

        long userId = ResolveUserId(conn, rating.Creator);

        using var cmd = new NpgsqlCommand(@"
            INSERT INTO ratings (id, media_id, user_id, stars, comment, is_comment_confirmed, created_at, updated_at)
            VALUES (@id, @m, @u, @s, @c, @cc, now(), now());", conn);

        cmd.Parameters.AddWithValue("id", rating.Id);
        cmd.Parameters.AddWithValue("m", (long)rating.MediaId);
        cmd.Parameters.AddWithValue("u", userId);
        cmd.Parameters.AddWithValue("s", rating.Stars);
        cmd.Parameters.AddWithValue("c", rating.Comment ?? string.Empty);
        cmd.Parameters.AddWithValue("cc", false);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            // UNIQUE (media_id, user_id)
            throw new InvalidOperationException("User already rated this media.");
        }
    }

    public Rating? Get(Guid id)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            SELECT r.id, r.media_id, u.username, r.stars, r.comment, r.created_at
            FROM ratings r
            JOIN users u ON u.id = r.user_id
            WHERE r.id = @id;", conn);

        cmd.Parameters.AddWithValue("id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        return Map(reader);
    }

    public bool Update(Rating rating)
    {
        using var conn = DbConnection.CreateAndOpen();

        long userId = ResolveUserId(conn, rating.Creator);

        using var cmd = new NpgsqlCommand(@"
            UPDATE ratings
            SET stars = @s,
                comment = @c,
                updated_at = now()
            WHERE id = @id AND user_id = @u;", conn);

        cmd.Parameters.AddWithValue("id", rating.Id);
        cmd.Parameters.AddWithValue("u", userId);
        cmd.Parameters.AddWithValue("s", rating.Stars);
        cmd.Parameters.AddWithValue("c", rating.Comment ?? string.Empty);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(Guid id)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand("DELETE FROM ratings WHERE id = @id;", conn);
        cmd.Parameters.AddWithValue("id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Lädt alle Ratings eines Mediums aus der Datenbank
    public IEnumerable<Rating> GetAllForMedia(int mediaId)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            SELECT r.id, r.media_id, u.username, r.stars, r.comment, r.created_at
            FROM ratings r
            JOIN users u ON u.id = r.user_id
            WHERE r.media_id = @m
            ORDER BY r.created_at DESC;", conn);

        cmd.Parameters.AddWithValue("m", (long)mediaId);

        var list = new List<Rating>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(Map(reader));

        return list;
    }

    // Liefert Durchschnittsbewertung und Anzahl der Ratings eines Mediums
    public (double Avg, int Count) GetStatsForMedia(int mediaId)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
        SELECT COALESCE(AVG(stars), 0)::double precision AS avg,
               COUNT(*)::int AS cnt
        FROM ratings
        WHERE media_id = @m;", conn);

        cmd.Parameters.AddWithValue("m", mediaId);

        using var r = cmd.ExecuteReader();
        r.Read();

        double avg = r.GetDouble(0);
        int cnt = r.GetInt32(1);

        return (avg, cnt);
    }


    // --- helpers ---

    // Ermittelt die User-ID zu einem gegebenen Benutzernamen
    private static long ResolveUserId(NpgsqlConnection conn, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("Creator must be set.");

        using var cmd = new NpgsqlCommand("SELECT id FROM users WHERE username = @u;", conn);
        cmd.Parameters.AddWithValue("u", username);

        var obj = cmd.ExecuteScalar();
        if (obj is null)
            throw new InvalidOperationException("User not found.");

        return (long)obj;
    }

    // Mappt einen Datenbank-Datensatz auf ein Rating-Objekt für Typesafety
    private static Rating Map(NpgsqlDataReader r)
    {
        var rating = new Rating();

        rating.Id = r.GetGuid(0);
        rating.MediaId = checked((int)r.GetInt64(1));
        rating.Creator = r.IsDBNull(2) ? string.Empty : r.GetString(2);
        rating.Stars = r.GetInt32(3);
        rating.Comment = r.IsDBNull(4) ? string.Empty : r.GetString(4);
        rating.Timestamp = r.GetDateTime(5);

        return rating;
    }
}
