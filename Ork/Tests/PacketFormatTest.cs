using Ork.Network;

namespace Ork.Tests
{
    public class PacketFormatTest
    {
        public static void Run()
        {
            string key = "Test.123!+=";
            string value = "Blah";

            string key2 = "TestEEEE";
            string value2 = "Whatataa";

            // Create a test packet
            Packet packet = new Packet(PacketType.Identify);
            packet[key] = value;
            packet[key2] = value2;

            // Get the byte array
            byte[] bytes = packet.GetBytes();

            // Create packet from a byte array
            Packet newPacket = Packet.GetPacketFromBuffer(bytes);
            if (newPacket[key] == value)
            {
                Console.WriteLine("(1/2) testing...");
            }
            else
            {
                Console.WriteLine($"Test Failed! Expected: {value} but got {newPacket[key]}");
                Console.ReadLine();
            }

            if (newPacket[key2] == value2)
            {
                Console.WriteLine("(2/2) Test Passed!");
            }
            else
            {
                Console.WriteLine($"Test Failed! Expected: {value2} but got {newPacket[key2]}");
                Console.ReadLine();
            }
        }
    }
}
