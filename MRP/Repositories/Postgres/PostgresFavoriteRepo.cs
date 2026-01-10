using Npgsql;
using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public sealed class PostgresFavoriteRepo : IFavoriteRepository
    {
        public bool Add(string username, int mediaId)
        {
            using var conn = DbConnection.CreateAndOpen();

            var userId = ResolveUserId(conn, username);

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO favorites (media_id, user_id, created_at)
                VALUES (@m, @u, now())
                ON CONFLICT DO NOTHING;", conn);

            cmd.Parameters.AddWithValue("m", (long)mediaId);
            cmd.Parameters.AddWithValue("u", userId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(string username, int mediaId)
        {
            using var conn = DbConnection.CreateAndOpen();

            var userId = ResolveUserId(conn, username);

            using var cmd = new NpgsqlCommand(@"
                DELETE FROM favorites
                WHERE media_id = @m AND user_id = @u;", conn);

            cmd.Parameters.AddWithValue("m", (long)mediaId);
            cmd.Parameters.AddWithValue("u", userId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool IsFavorite(string username, int mediaId)
        {
            using var conn = DbConnection.CreateAndOpen();

            var userId = ResolveUserId(conn, username);

            using var cmd = new NpgsqlCommand(@"
                SELECT 1
                FROM favorites
                WHERE media_id = @m AND user_id = @u
                LIMIT 1;", conn);

            cmd.Parameters.AddWithValue("m", (long)mediaId);
            cmd.Parameters.AddWithValue("u", userId);

            var res = cmd.ExecuteScalar();
            return res != null;
        }

        public IReadOnlyCollection<int> GetFavoriteMediaIds(string username)
        {
            using var conn = DbConnection.CreateAndOpen();

            var userId = ResolveUserId(conn, username);

            using var cmd = new NpgsqlCommand(@"
                SELECT media_id
                FROM favorites
                WHERE user_id = @u
                ORDER BY created_at DESC, media_id;", conn);

            cmd.Parameters.AddWithValue("u", userId);

            var list = new List<int>();
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(checked((int)r.GetInt64(0)));
            }

            return list.AsReadOnly();
        }

        public void Clear()
        {
            using var conn = DbConnection.CreateAndOpen();
            using var cmd = new NpgsqlCommand("TRUNCATE TABLE favorites;", conn);
            cmd.ExecuteNonQuery();
        }

        //Ermittelt die Datenbank-User-ID zu einem Benutzernamen
        private static long ResolveUserId(NpgsqlConnection conn, string username)
        {
            username ??= string.Empty;

            using var cmd = new NpgsqlCommand(@"
                SELECT id
                FROM users
                WHERE username = @un;", conn);

            cmd.Parameters.AddWithValue("un", username);

            var obj = cmd.ExecuteScalar();
            if (obj == null)
                throw new InvalidOperationException($"Unknown user '{username}'.");

            return (long)obj;
        }
    }
}
