namespace DSMOOServer.Network;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class PacketAttribute : Attribute
{
    public PacketAttribute(PacketType type)
    {
        Id = (short)type;
    }
    
    public PacketAttribute(short id)
    {
        Id = id;
    }

    public short Id { get; }
}