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
        }

        private void HandlePacket(Packet packet)
        {
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

                case PacketType.Sensor:
                    HandleSensorPacket(packet);
                    break;

                case PacketType.Controller:
                    HandleControllerPacket(packet);
                    break;

                case PacketType.Browse:
                    _ = HandleBrowsePacket(packet);
                    break;
            }
        }

        private async Task HandleBrowsePacket(Packet packet)
        {
            Packet response = new Packet(PacketType.Browse);

            List<DatabaseLevelEntry> levels = await Database.GetLevels(packet.GetStringField("request"));

            response.SetStringField("count", $"{levels.Count}");
            for (int i = 0; i < levels.Count; i++)
            {
                response.SetStringField($"level.{i}.id", levels[i].level_id);
            }

            _ = Connection.SendPacket(response);
        }

        private void HandleControllerPacket(Packet packet)
        {
            Bridge? bridge = BridgeManager.GetBridge(this);
            if (bridge == null)
            {
                Connection.SendError("Not Bridged");
                return;
            }

            if (userDevice == UserDevice.Phone)
            {
                bridge.Game.Connection.SendPacket(packet);
            }
            else
            {
                Connection.SendError("Invalid Device!");
            }
        }

        private void HandleSensorPacket(Packet packet)
        {
            Bridge? bridge = BridgeManager.GetBridge(this);
            if (bridge == null)
            {
                Connection.SendError("Not Bridged");
                return;
            }

            if (userDevice == UserDevice.Phone)
            {
                bridge.Game.Connection.SendPacket(packet);
            }

            else if (userDevice == UserDevice.Game)
            {
                bridge.Phone.Connection.SendPacket(packet);
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
