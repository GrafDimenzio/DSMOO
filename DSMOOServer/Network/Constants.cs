using System.Reflection;

namespace DSMOOServer.Network;

public static class Constants
{
    public const int CostumeNameSize = 0x20;

    public static int HeaderSize { get; } = PacketHeader.StaticSize;
}