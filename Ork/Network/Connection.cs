using System.Net.Sockets;

namespace Ork.Network
{
    public class Connection
    {
        private TcpClient client;
        private bool connected;
        public Server server { get; private set; }
        public Connection(TcpClient client, Server server)
        {
            this.server = server;
            this.client = client;
            connected = true;
            OnDisconnect += () =>
            {
                connected = false;
                server.Disconnect(this);
            };
        }

        public void SendPacket(Packet packet)
        {
            if (!connected)
                return;
            try
            {
                byte[] packetData = packet.GetBytes();
                byte[] lengthData = BitConverter.GetBytes(packetData.Length);

                // Write packet header
                client.GetStream().Write(lengthData);
                client.GetStream().Write(packetData, 0, packetData.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Disconnected user while sending packet because {e}");
            }
        }

        public Action? OnDisconnect;

        public void SendError(string message)
        {
            Packet errorPacket = new Packet(PacketType.Error);
            errorPacket["message"] = message;

            SendPacket(errorPacket);
        }

        private int size = -1;
        public bool Pending()
        {
            if (!connected)
                return false;

            try
            {
                int available = client.Available;

                if (size == -1 && available >= 4)
                {
                    byte[] buffer = new byte[4];
                    client.GetStream().Read(buffer);

                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(buffer);
                    }

                    size = BitConverter.ToInt32(buffer, 0);
                }

                available = client.Available;
                return size != -1 && available >= size;
            }
            catch (Exception e)
            {
                OnDisconnect?.Invoke();
                Console.WriteLine($"Disconnected user while waiting for next packet because {e}");
                return false;
            }
        }

        public void AcceptPacket()
        {
            if (!connected)
                return;

            if (!Pending())
            {
                return;
            }

            Packet packet;
            try
            {
                byte[] buffer = new byte[size];
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);

                client.GetStream().Read(buffer, 0, size);

                packet = Packet.GetPacketFromBuffer(buffer);
                size = -1;
            }
            catch (Exception e)
            {
                OnDisconnect?.Invoke();
                Console.WriteLine($"Disconnected user while reading packet because {e}");
                return;
            }

            OnPacket?.Invoke(packet);
        }

        public Action<Packet>? OnPacket;
    }
}
