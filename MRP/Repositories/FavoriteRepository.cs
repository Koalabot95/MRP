using System.Collections.Generic;
using System.Linq;

namespace FHTW.Swen1.Forum.System
{
    public sealed class FavoriteRepository : IFavoriteRepository
    {
        // Key: username, Value: set of mediaIds
        private readonly Dictionary<string, HashSet<int>> _favorites = new();

        public bool Add(string username, int mediaId)
        {
            username ??= string.Empty;

            if (!_favorites.TryGetValue(username, out var set))
            {
                set = new HashSet<int>();
                _favorites[username] = set;
            }

            return set.Add(mediaId);
        }

        public bool Remove(string username, int mediaId)
        {
            username ??= string.Empty;

            return _favorites.TryGetValue(username, out var set) && set.Remove(mediaId);
        }

        public bool IsFavorite(string username, int mediaId)
        {
            username ??= string.Empty;

            return _favorites.TryGetValue(username, out var set) && set.Contains(mediaId);
        }

        public IReadOnlyCollection<int> GetFavoriteMediaIds(string username)
        {
            username ??= string.Empty;

            if (!_favorites.TryGetValue(username, out var set))
                return new List<int>().AsReadOnly();

            return set.OrderBy(x => x).ToList().AsReadOnly();
        }

        public void Clear()
            => _favorites.Clear();
    }
}
