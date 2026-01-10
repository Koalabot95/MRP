using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public interface IFavoriteRepository
    {
        // markiert ein Medium als Favorit für den User
        // return true = neu angelegt, false = war schon Favorit
        bool Add(string username, int mediaId);

        // entfernt Favorit
        // return true = entfernt, false = war nicht vorhanden
        bool Remove(string username, int mediaId);

        // prüft ob Favorit gesetzt ist
        bool IsFavorite(string username, int mediaId);

        // liefert alle favorisierten mediaIds des Users
        IReadOnlyCollection<int> GetFavoriteMediaIds(string username);

        void Clear();
    }
}
