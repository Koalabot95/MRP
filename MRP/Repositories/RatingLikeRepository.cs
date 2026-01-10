using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public sealed class RatingLikeRepository : IRatingLikeRepository
    {
        // ratingId -> set(usernames)
        private readonly Dictionary<Guid, HashSet<string>> _likes = new();

        public bool Add(string username, Guid ratingId)
        {
            username ??= string.Empty;

            if (!_likes.TryGetValue(ratingId, out var set))
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                _likes[ratingId] = set;
            }

            return set.Add(username);
        }

        public bool Remove(string username, Guid ratingId)
        {
            username ??= string.Empty;

            return _likes.TryGetValue(ratingId, out var set) && set.Remove(username);
        }

        public bool IsLiked(string username, Guid ratingId)
        {
            username ??= string.Empty;

            return _likes.TryGetValue(ratingId, out var set) && set.Contains(username);
        }

        public int Count(Guid ratingId)
        {
            return _likes.TryGetValue(ratingId, out var set) ? set.Count : 0;
        }

        public void Clear() => _likes.Clear();
    }
}
