using Ork.Network;
using Ork.Users;

namespace Ork.Bridges
{
    public class Bridge
    {
        public string Code { get; private set; }

        public User? Game { get; private set; }
        public User? Phone { get; private set; }

        public void SetGameUser(User user)
        {
            Game = user;
        }

        public void SetPhoneUser(User user)
        {
            Phone = user;
        }

        public Bridge(string code)
        {
            this.Code = code;
        }

        public void Ready()
        {
            Packet packet = new Packet(PacketType.Ready);
            Game?.Connection.SendPacket(packet);
            Phone?.Connection.SendPacket(packet);

            Console.WriteLine("Bridge is Ready!");
        }
    }
}
