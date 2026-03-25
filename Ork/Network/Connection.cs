using Ork;
using Ork.Network;
using System.Net.Sockets;

public class Connection
{
    private TcpClient client;
    private bool connected;
    public Connection(TcpClient client)
    {
        this.client = client;
        connected = true;
        OnDisconnect += () =>
        {
            connected = false;
        };

        Task.Run(() => ReceiveLoop(client.GetStream()));
    }

    /// <summary>
    /// Send a packet over the network
    /// </summary>
    /// <param name="packet"></param>
    public async Task SendPacket(Packet packet)
    {
        if (!connected)
            return;
        try
        {
            byte[] packetData = packet.GetBytes();
            byte[] lengthData = BitConverter.GetBytes(packetData.Length);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthData);
            }

            // Write packet header
            await client.GetStream().WriteAsync(lengthData);
            await client.GetStream().WriteAsync(packetData, 0, packetData.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Disconnected user while sending packet because {e}");
        }
    }

    public Action OnDisconnect;

    private async Task ReceiveLoop(NetworkStream stream)
    {
        try
        {
            while (connected)
            {
                byte[] lengthBuffer = new byte[4];
                await ReadExactAsync(stream, lengthBuffer, 4);

                int size = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] buffer = new byte[size];
                await ReadExactAsync(stream, buffer, size);

                Packet packet = Packet.GetPacketFromBuffer(buffer);

                MainThread.Run(() =>
                {
                    OnPacket?.Invoke(packet);
                });
            }
        }
        catch (Exception e)
        {
            OnDisconnect?.Invoke();
            Console.WriteLine($"Disconnected: {e}");
        }
    }

    private async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int size)
    {
        int totalRead = 0;
        while (totalRead < size)
        {
            int read = await stream.ReadAsync(buffer, totalRead, size - totalRead);
            if (read == 0)
                throw new Exception("Disconnected");
            totalRead += read;
        }
    }

    /// <summary>
    /// Get's called when a packet has been revieved
    /// </summary>
    public Action<Packet> OnPacket;
}
