using System.Runtime.InteropServices;
using System.Text;
using DSMOOPlus.Enum;
using DSMOOServer;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(13)]
public struct MessagePacket : IPacket
{
    public const int MessageSize = 0x4B;

    public MessagePacket()
    {
    }

    public uint SenderId = 0;
    public MessageType MessageType = MessageType.Chat;
    public string Message = string.Empty;

    public short Size => sizeof(uint) + sizeof(MessageType) + MessageSize;

    public void Serialize(Span<byte> data)
    {
        if (data.Length >= sizeof(uint))
            MemoryMarshal.Write(data, SenderId);
        
        if (data.Length >= (sizeof(uint) + sizeof(MessageType)))
            MemoryMarshal.Write(data[sizeof(uint)..], MessageType);

        if (data.Length >= (sizeof(uint) + sizeof(MessageType) + MessageSize))
            Encoding.ASCII.GetBytes(Message).CopyTo(data[(sizeof(uint) + sizeof(MessageType))..]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        SenderId = MemoryMarshal.Read<uint>(data);
        MessageType = MemoryMarshal.Read<MessageType>(data[sizeof(uint)..]);
        Message = Encoding.ASCII.GetString(data[(sizeof(uint) + sizeof(MessageType))..]).TrimNullTerm();
    }
}