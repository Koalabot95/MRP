using System.Collections.Generic;
using System.Linq;
using FHTW.Swen1.Forum.System;

namespace FHTW.Swen1.Forum.System
{
    public class MediaRepository : IMediaRepository
    {
        private readonly Dictionary<int, Media> _media = new();
        private int _nextId = 1;
        
        //wenn keine id -> neue id. media speichern
        public Media Add(Media media)
        {
            if (media.Id == 0)
            {
                media.Id = _nextId++;
            }

            _media[media.Id] = media;
            return media;
        }

        //einzelnes Medium holen
        public Media? Get(int id)
            => _media.TryGetValue(id, out var media) ? media : null;
        
        //liste aller medien holen
        public IReadOnlyCollection<Media> GetAll()
            => _media.Values.ToList().AsReadOnly();

        //wenn id besteht -> überschreiben
        public Media? Update(Media media)
        {
            if (!_media.ContainsKey(media.Id))
            {
                return null;
            }

            _media[media.Id] = media;
            return media;
        }

        public bool Delete(int id)
            => _media.Remove(id);

        public void Clear()
        {
            _media.Clear();
            _nextId = 1;
        }
    }
}
