using System.Runtime.InteropServices;
using System.Text;
using DSMOOPlus.Enum;
using DSMOOServer;
using DSMOOServer.Network;

namespace DSMOOPlus.Packets;

[Packet(13)]
public struct SendMessagePacket : IPacket
{
    public const int MessageSize = 0x4B;
    
    public SendMessagePacket() { }
    
    public uint SenderId { get; set; } = 0;

    public MessageType MessageType { get; set; } = MessageType.Chat;
    
    public string Message { get; set; } = string.Empty;
    
    public short Size => 8 + MessageSize;

    public void Serialize(Span<byte> data)
    {
        MemoryMarshal.Write(data, SenderId);
        MemoryMarshal.Write(data[4..], MessageType);
        Encoding.ASCII.GetBytes(Message).CopyTo(data[8..]);
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        SenderId = MemoryMarshal.Read<uint>(data);
        MessageType = MemoryMarshal.Read<MessageType>(data[4..]);
        Message = Encoding.ASCII.GetString(data[8..]).TrimNullTerm();
    }
}