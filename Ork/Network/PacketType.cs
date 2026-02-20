namespace Ork.Network
{
    public enum PacketType : byte
    {
        Identify = 0,
        Portal = 1,
        DisplayCode = 2,
        UseCode = 3,
        Error = 4,
        Ready = 5
    }
}
