using System.Text;
namespace Ork.Network;

public class Packet
{

    private const int MAX_PACKET_FIELD_COUNT = 1024;
    private const int MAX_PACKET_FIELD_SIZE = 1024 * 10; // I don't think this will ever be passed.
    private const int MAX_PACKET_VALUE_SIZE = 1024 * 1024 * 100; // 100MB
    public PacketType PacketType { get; private set; }
    private Dictionary<string, byte[]> Fields { get; set; } = new Dictionary<string, byte[]>();

    public Packet(PacketType packetType)
    {
        PacketType = packetType;
    }

    public static Packet GetPacketFromBuffer(byte[] buffer)
    {
        int pointer = 0;

        if (buffer.Length == 0)
            throw new Exception("Invalid packet: Empty buffer");

        if (!Enum.IsDefined(typeof(PacketType), buffer[pointer]))
            throw new Exception("Invalid packet type");

        Packet packet = new Packet((PacketType)buffer[pointer]);
        pointer++;

        if (pointer + 4 > buffer.Length)
            throw new Exception("Invalid packet: missing field count");

        int fieldCount = BitConverter.ToInt32(buffer, pointer);

        if (fieldCount > MAX_PACKET_FIELD_COUNT)
            throw new Exception($"Invalid packet: MAX_PACKET_FIELD_COUNT has been surpassed. {fieldCount} > {MAX_PACKET_FIELD_COUNT}");

        if (fieldCount < 0)
            throw new Exception("Invalid packet: Field Count can not be less than 0.");

        pointer += 4;

        for (int i = 0; i < fieldCount; i++)
        {
            if (pointer + 4 > buffer.Length)
                throw new Exception("Invalid packet: missing key size");

            int keySize = BitConverter.ToInt32(buffer, pointer);
            pointer += 4;

            if (keySize > MAX_PACKET_FIELD_SIZE)
                throw new Exception($"Invalid packet: MAX_PACKET_FIELD_SIZE has been surpassed. {keySize} > {MAX_PACKET_FIELD_SIZE}");

            if (pointer + keySize > buffer.Length)
                throw new Exception("Invalid packet: key exceeds buffer");

            if (keySize < 0)
                throw new Exception($"Invalid packet: keySize can not be negative! {keySize}.");

            string key = Encoding.UTF8.GetString(buffer, pointer, keySize);
            pointer += keySize;

            if (pointer + 4 > buffer.Length)
                throw new Exception("Invalid packet: missing value size");


            int valueSize = BitConverter.ToInt32(buffer, pointer);
            pointer += 4;

            if (valueSize < 0)
                throw new Exception($"Invalid packet: valueSize can not be negative! {valueSize}.");

            if (valueSize > MAX_PACKET_VALUE_SIZE)
                throw new Exception($"Invalid packet: MAX_PACKET_VALUE_SIZE has been surpassed. {valueSize} > {MAX_PACKET_VALUE_SIZE}");

            if (pointer + valueSize > buffer.Length)
                throw new Exception("Invalid packet: value exceeds buffer");


            byte[] value = buffer.AsSpan(pointer, valueSize).ToArray();
            pointer += valueSize;

            packet.SetBytesField(key, value);
        }

        return packet;
    }

    public byte[] GetBytes()
    {
        MemoryStream memoryStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memoryStream);

        // Write packet type 
        writer.Write((byte)PacketType);

        writer.Write(Fields.Count);

        foreach (var kvp in Fields)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(kvp.Key);
            byte[] valueBytes = kvp.Value;

            writer.Write(keyBytes.Length);
            writer.Write(keyBytes);

            writer.Write(valueBytes.Length);
            writer.Write(valueBytes);
        }

        return memoryStream.ToArray();
    }

    public string this[string field]
    {
        get => GetStringField(field);
        set => SetStringField(field, value);
    }

    public string GetStringField(string field)
    {
        return Encoding.UTF8.GetString(GetBytesField(field));
    }

    public byte[] GetBytesField(string field)
    {
        return Fields[field];
    }

    public void SetBytesField(string field, byte[] value)
    {
        Fields[field] = value;
    }

    public void SetStringField(string field, string value)
    {
        byte[] data = Encoding.UTF8.GetBytes(value);
        SetBytesField(field, data);
    }
}
