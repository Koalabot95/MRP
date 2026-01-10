/*using MRP.System;
using System;
using System.Collections.Generic;
using System.Linq;*/

namespace FHTW.Swen1.Forum.System
{
    public class RatingRepository : IRatingRepository
    {
        //key Guid, value = Rating Objekt
        private static readonly Dictionary<Guid, Rating> _ratings = new();

        public void Add(Rating rating)
            => _ratings.Add(rating.Id, rating);

        //holt rating id
        public Rating? Get(Guid id)
            => _ratings.TryGetValue(id, out var r) ? r : null;

        public bool Update(Rating rating)
        {
            if (!_ratings.ContainsKey(rating.Id))
                return false;

            _ratings[rating.Id] = rating;
            return true;
        }

        public bool Delete(Guid id)
            => _ratings.Remove(id);

        //Holt alle Ratings zu einem bestimmten Media
        public IEnumerable<Rating> GetAllForMedia(int mediaId)
            => _ratings.Values.Where(r => r.MediaId == mediaId);

        public (double Avg, int Count) GetStatsForMedia(int mediaId)
        {
            var list = _ratings.Values.Where(r => r.MediaId == mediaId).ToList();
            if (list.Count == 0) return (0.0, 0);

            double avg = list.Average(r => r.Stars);
            return (avg, list.Count);
        }

        //für Tests
        public void Clear() => _ratings.Clear();
    }
}
