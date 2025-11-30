namespace DSMOOServer.Network;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class PacketAttribute : Attribute {
    public PacketAttribute(PacketType type) {
        Type = type;
    }

    public PacketType Type { get; }
}