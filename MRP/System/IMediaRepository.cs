using FHTW.Swen1.Forum.System;

namespace FHTW.Swen1.Forum.System
{
    public interface IMediaRepository
    {
        Media Add(Media media);
        Media? Get(int id);
        IReadOnlyCollection<Media> GetAll();
        Media? Update(Media media);
        bool Delete(int id);
        void Clear();
    }
}
