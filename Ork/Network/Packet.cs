using System.Text;
using System.Text.Json;

namespace Ork.Network
{
    public class Packet
    {
        public PacketType PacketType { get; private set; }
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

        public Packet(PacketType packetType)
        {
            PacketType = packetType;
        }

        public static Packet GetPacketFromBuffer(byte[] buffer)
        {
            int pointer = 0;

            if (buffer.Length == 0)
                throw new Exception("Empty buffer");

            Packet packet = new Packet((PacketType)buffer[pointer]);
            pointer++;

            string? field = null;

            while (pointer < buffer.Length)
            {
                if (pointer >= buffer.Length)
                    break;

                byte size = buffer[pointer];
                pointer++;

                if (pointer + size > buffer.Length)
                    break;

                string result = Encoding.UTF8.GetString(buffer, pointer, size);

                if (field == null)
                    field = result;
                else
                {
                    packet.SetValue(field, result);
                    field = null;
                }

                pointer += size;
            }

            return packet;
        }

        public byte[] GetBytes()
        {
            MemoryStream memoryStream = new MemoryStream();

            // Write packet type
            memoryStream.WriteByte((byte)PacketType);

            foreach (string key in Fields.Keys)
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] valueBytes = Encoding.UTF8.GetBytes(Fields[key]);

                // Write key text
                memoryStream.WriteByte((byte)keyBytes.Length);
                memoryStream.Write(keyBytes);

                // Write value text 
                memoryStream.WriteByte((byte)valueBytes.Length);
                memoryStream.Write(valueBytes);
            }

            return memoryStream.ToArray();
        }

        public string this[string field]
        {
            get => GetValue(field);
            set => SetValue(field, value);
        }

        public string GetValue(string field)
        {
            return Fields[field];
        }

        public void SetValue(string field, string value)
        {
            if (!Fields.ContainsKey(field))
            {
                Fields.Add(field, value);
                return;
            }

            Fields[field] = value;
        }

        public string ToJson()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            return JsonSerializer.Serialize(this, options);
        }
    }
}
