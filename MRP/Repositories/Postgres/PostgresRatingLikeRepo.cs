using Npgsql;
using System;

namespace FHTW.Swen1.Forum.System
{
    public sealed class PostgresRatingLikeRepo : IRatingLikeRepository
    {
        public bool Add(string username, Guid ratingId)
        {
            using var conn = DbConnection.CreateAndOpen();

            EnsureRatingExists(conn, ratingId);

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO rating_likes (rating_id, username, created_at)
                VALUES (@r, @u, now())
                ON CONFLICT DO NOTHING;", conn);

            cmd.Parameters.AddWithValue("r", ratingId);
            cmd.Parameters.AddWithValue("u", username ?? string.Empty);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(string username, Guid ratingId)
        {
            using var conn = DbConnection.CreateAndOpen();

            using var cmd = new NpgsqlCommand(@"
                DELETE FROM rating_likes
                WHERE rating_id = @r AND username = @u;", conn);

            cmd.Parameters.AddWithValue("r", ratingId);
            cmd.Parameters.AddWithValue("u", username ?? string.Empty);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool IsLiked(string username, Guid ratingId)
        {
            using var conn = DbConnection.CreateAndOpen();

            using var cmd = new NpgsqlCommand(@"
                SELECT 1
                FROM rating_likes
                WHERE rating_id = @r AND username = @u
                LIMIT 1;", conn);

            cmd.Parameters.AddWithValue("r", ratingId);
            cmd.Parameters.AddWithValue("u", username ?? string.Empty);

            return cmd.ExecuteScalar() != null;
        }

        public int Count(Guid ratingId)
        {
            using var conn = DbConnection.CreateAndOpen();

            using var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*)
                FROM rating_likes
                WHERE rating_id = @r;", conn);

            cmd.Parameters.AddWithValue("r", ratingId);

            var obj = cmd.ExecuteScalar();
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        public void Clear()
        {
            using var conn = DbConnection.CreateAndOpen();
            using var cmd = new NpgsqlCommand("TRUNCATE TABLE rating_likes;", conn);
            cmd.ExecuteNonQuery();
        }

        private static void EnsureRatingExists(NpgsqlConnection conn, Guid ratingId)
        {
            using var cmd = new NpgsqlCommand(@"
                SELECT 1 FROM ratings WHERE id = @r LIMIT 1;", conn);
            cmd.Parameters.AddWithValue("r", ratingId);

            if (cmd.ExecuteScalar() == null)
                throw new InvalidOperationException("Rating not found.");
        }
    }
}
