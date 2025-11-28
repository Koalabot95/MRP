using System.Collections.Generic;

namespace FHTW.Swen1.Forum.System
{
    public interface IUserRepository
    {
        void Add(User user);
        User? Get(string UserName);
        bool Update(User user);
        bool Delete(string UserName);
    }
}
