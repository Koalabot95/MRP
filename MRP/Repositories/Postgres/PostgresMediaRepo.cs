using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FHTW.Swen1.Forum.System;

public sealed class PostgresMediaRepo : IMediaRepository
{
    public Media Add(Media media)
    {
        using var conn = DbConnection.CreateAndOpen();

        // InMemory-Repo vergibt Id, wenn Id == 0.
        // In Postgres ist id BIGSERIAL -> holen die neue Id mit RETURNING.
        if (media.Id == 0)
        {
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO media (title, description, type, release_year, genres, age_restriction, owner_username, created_at, updated_at)
                VALUES (@t, @d, @ty, @y, @g, @ar, @ou, now(), now())
                RETURNING id;", conn);

            cmd.Parameters.AddWithValue("t", media.Title ?? string.Empty);
            cmd.Parameters.AddWithValue("d", media.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("ty", media.Type.ToString()); // enum -> Movie/Series/Game
            cmd.Parameters.AddWithValue("y", media.ReleaseYear);
            cmd.Parameters.AddWithValue("g", media.Genres ?? string.Empty);
            cmd.Parameters.AddWithValue("ar", media.AgeRestriction);
            cmd.Parameters.AddWithValue("ou", media.OwnerUserName ?? string.Empty);

            var newId = (long)cmd.ExecuteScalar()!;
            media.Id = checked((int)newId);
            return media;
        }

        var updated = Update(media);
        if (updated != null) return updated;

        using (var cmd = new NpgsqlCommand(@"
            INSERT INTO media (id, title, description, type, release_year, genres, age_restriction, owner_username, created_at, updated_at)
            VALUES (@id, @t, @d, @ty, @y, @g, @ar, @ou, now(), now())
            RETURNING id;", conn))
        {
            cmd.Parameters.AddWithValue("id", media.Id);
            cmd.Parameters.AddWithValue("t", media.Title ?? string.Empty);
            cmd.Parameters.AddWithValue("d", media.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("ty", media.Type.ToString());
            cmd.Parameters.AddWithValue("y", media.ReleaseYear);
            cmd.Parameters.AddWithValue("g", media.Genres ?? string.Empty);
            cmd.Parameters.AddWithValue("ar", media.AgeRestriction);
            cmd.Parameters.AddWithValue("ou", media.OwnerUserName ?? string.Empty);

            cmd.ExecuteScalar();
        }

        return media;
    }

    // Lädt ein einzelnes Medium anhand seiner ID aus der Datenbank
    public Media? Get(int id)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            SELECT id, title, description, type, release_year, genres, age_restriction, owner_username
            FROM media
            WHERE id = @id;", conn);

        cmd.Parameters.AddWithValue("id", id);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return Map(r);
    }

    // Lädt alle Medien sortiert nach ID aus der Datenbank
    public IReadOnlyCollection<Media> GetAll()
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            SELECT id, title, description, type, release_year, genres, age_restriction, owner_username
            FROM media
            ORDER BY id;", conn);

        var list = new List<Media>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(Map(r));

        return list.AsReadOnly();
    }

    // Aktualisiert ein bestehendes Medium in der Datenbank
    public Media? Update(Media media)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand(@"
            UPDATE media
            SET title = @t,
                description = @d,
                type = @ty,
                release_year = @y,
                genres = @g,
                age_restriction = @ar,
                owner_username = @ou,
                updated_at = now()
            WHERE id = @id;", conn);

        cmd.Parameters.AddWithValue("id", media.Id);
        cmd.Parameters.AddWithValue("t", media.Title ?? string.Empty);
        cmd.Parameters.AddWithValue("d", media.Description ?? string.Empty);
        cmd.Parameters.AddWithValue("ty", media.Type.ToString());
        cmd.Parameters.AddWithValue("y", media.ReleaseYear);
        cmd.Parameters.AddWithValue("g", media.Genres ?? string.Empty);
        cmd.Parameters.AddWithValue("ar", media.AgeRestriction);
        cmd.Parameters.AddWithValue("ou", media.OwnerUserName ?? string.Empty);

        return cmd.ExecuteNonQuery() > 0 ? media : null;
    }

    // Löscht ein Medium anhand seiner ID aus der Datenbank
    public bool Delete(int id)
    {
        using var conn = DbConnection.CreateAndOpen();

        using var cmd = new NpgsqlCommand("DELETE FROM media WHERE id = @id;", conn);
        cmd.Parameters.AddWithValue("id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Löscht alle Medien und setzt die ID-Sequenz zurück
    public void Clear()
    {
        using var conn = DbConnection.CreateAndOpen();
        using var cmd = new NpgsqlCommand("TRUNCATE TABLE media RESTART IDENTITY;", conn);
        cmd.ExecuteNonQuery();
    }

    // trennt Datenbankrepräsentation von Domänenobjekten und verhindert, dass SQL-Details in die Business-Logik kommen
    private static Media Map(NpgsqlDataReader r)
    {
        var m = new Media();

        m.Id = checked((int)r.GetInt64(0));
        m.Title = r.IsDBNull(1) ? string.Empty : r.GetString(1);
        m.Description = r.IsDBNull(2) ? string.Empty : r.GetString(2);

        var typeStr = r.IsDBNull(3) ? "Movie" : r.GetString(3);
        m.Type = Enum.TryParse<MediaType>(typeStr, ignoreCase: true, out var mt) ? mt : MediaType.movie;

        m.ReleaseYear = r.IsDBNull(4) ? 0 : r.GetInt32(4);
        m.Genres = r.IsDBNull(5) ? string.Empty : r.GetString(5);
        m.AgeRestriction = r.IsDBNull(6) ? 0 : r.GetInt32(6);
        m.OwnerUserName = r.IsDBNull(7) ? string.Empty : r.GetString(7);

        return m;
    }
}
