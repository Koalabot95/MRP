using MRP.System;
using System;
using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public interface IRatingRepository
    {
        void Add(Rating rating);

        Rating? Get(Guid id);

        bool Update(Rating rating);

        bool Delete(Guid id);

        IEnumerable<Rating> GetAllForMedia(int mediaId);
    }
}