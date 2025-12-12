namespace DSMOOServer.Network;

public static class PacketHelper
{
    public static void FillPacket(PacketHeader header, IPacket packet, Memory<byte> memory)
    {
        var data = memory.Span;

        header.Serialize(data[..Constants.HeaderSize]);
        packet.Serialize(data[Constants.HeaderSize..]);
    }
}