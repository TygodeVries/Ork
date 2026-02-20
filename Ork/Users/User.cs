using Ork.Bridges;
using Ork.Network;

namespace Ork.Users
{
    public class User
    {
        public Connection Connection { get; private set; }
        public User(Connection connection)
        {
            this.Connection = connection;
            Assign();
        }

        public UserDevice? userDevice { get; private set; } = null;

        private void Assign()
        {
            Connection.OnPacket += HandlePacket;
            Connection.OnDisconnect += Disconnect;
        }

        private void Disconnect()
        {
            Console.WriteLine("User Disconnected.");
            Bridge? bridge = BridgeManager.GetBridge(this);
            if (bridge != null)
            {
                if (bridge.Game == this)
                    bridge.Phone?.Connection.SendError("Connection with other user lost.");

                if (bridge.Phone == this)
                    bridge.Game?.Connection.SendError("Connection with other user lost.");

                BridgeManager.Remove(bridge);
            }
        }

        private void HandlePacket(Packet packet)
        {
            Console.WriteLine(packet.ToJson());
            switch (packet.PacketType)
            {
                case PacketType.Identify:
                    HandleIdentifyPacket(packet);
                    break;

                case PacketType.UseCode:
                    HandleUseCodePacket(packet);
                    break;

                case PacketType.Portal:
                    HandlePortalPacket(packet);
                    break;
            }
        }

        private void HandlePortalPacket(Packet packet)
        {
            Bridge? bridge = BridgeManager.GetBridge(this);
            if (bridge == null)
            {
                Connection.SendError("Not Bridged");
                return;
            }

            //#TODO Add more data from database, check for validity, etc

            bridge.Game.Connection.SendPacket(packet);
        }

        private void HandleUseCodePacket(Packet packet)
        {
            if (userDevice != UserDevice.Phone)
            {
                Connection.SendError("Wrong Device");
                Console.WriteLine("Wrong Device!");
                return;
            }

            Bridge? bridge = BridgeManager.GetBridge(packet["code"]);
            if (bridge == null)
            {
                Connection.SendError("Invalid Code");
                Console.WriteLine("Invalid Code!");
                return;
            }

            bridge.SetPhoneUser(this);
            bridge.Ready();
        }

        private void HandleIdentifyPacket(Packet packet)
        {
            if (userDevice != null)
            {
                Connection.SendError("Already Identified.");
                return;
            }

            if (packet["device"] == "phone")
            {
                userDevice = UserDevice.Phone;
            }

            if (packet["device"] == "game")
            {
                userDevice = UserDevice.Game;

                // Create a bridge
                Bridge bridge = BridgeManager.CreateBridge();
                bridge.SetGameUser(this);

                // Respond with the code
                Packet displayCodePacket = new Packet(PacketType.DisplayCode);
                displayCodePacket["code"] = bridge.Code;

                Connection.SendPacket(displayCodePacket);
            }

            Console.WriteLine($"User identified as {userDevice}.");
        }
    }
}
