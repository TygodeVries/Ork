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
                User user = new User(connection);
                AddUser(user);
            };
        }
    }
}
