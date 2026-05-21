using Ork.Network;

namespace Ork.Users
{
    public class UserManager
    {
        private List<User> users = new List<User>();
        public void AddUser(User user)
        {
            users.Add(user);
            Console.WriteLine("User Connected.");
        }

        public void UseServer(Server server)
        {
            server.AcceptNewClient += (Connection connection) =>
            {
                connection.SetUserManager(this);
                User user = new User(connection);
                AddUser(user);
            };
        }

        public User? GetUser(Connection connection)
        {
            foreach (var user in users)
            {
                if (user.Connection == connection)
                    return user;
            }

            return null;
        }
    }
}
