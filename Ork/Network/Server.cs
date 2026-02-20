using System.Net;
using System.Net.Sockets;

namespace Ork.Network
{
    public class Server
    {
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        public Server(IPAddress address, int port)
        {
            this.Address = address;
            this.Port = port;
        }

        private TcpListener? listener;
        public void Start()
        {
            Console.WriteLine("Starting server...");

            listener = new TcpListener(Address, Port);
            listener.Start();

            Console.WriteLine("Server Ready.");
        }

        public void Disconnect(Connection connection)
        {
            connections.Remove(connection);
        }

        public void Stop()
        {
            listener?.Stop();
        }

        public void Tick()
        {
            if (listener == null)
            {
                throw new InvalidOperationException("Must call Start() before ticking!");
            }

            // Check if there are any new connections pending
            bool pending = listener.Pending();
            if (pending)
            {
                TcpClient network = listener.AcceptTcpClient();
                Connection connection = new Connection(network, this);
                AcceptNewClient?.Invoke(connection);
                connections.Add(connection);
            }

            foreach (Connection connection in connections)
            {
                connection.AcceptPacket();
            }
        }

        private List<Connection> connections = new List<Connection>();
        public Action<Connection>? AcceptNewClient;
    }
}
