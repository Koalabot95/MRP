using System;

namespace FHTW.Swen1.Forum.System
{
    public interface IRatingLikeRepository
    {
        bool Add(string username, Guid ratingId);       // true = neu geliked, false = schon vorhanden
        bool Remove(string username, Guid ratingId);    // true = entfernt, false = war nicht vorhanden
        bool IsLiked(string username, Guid ratingId);   // true/false
        int Count(Guid ratingId);                       // like count
        void Clear();
    }
}
