namespace FHTW.Swen1.Forum.System
{
    public class UserRepository : IUserRepository
    {
        private static readonly Dictionary<string, User> _users = new();

        public void Add(User user)
            => _users[user.UserName] = user;

        public User? Get(string UserName)
            => _users.TryGetValue(UserName, out var user) ? user : null;

        public bool Update(User user)
        {
            if (!_users.ContainsKey(user.UserName))
                return false;

            _users[user.UserName] = user;
            return true;
        }

        public bool Delete(string UserName)
            => _users.Remove(UserName);

    }
}
