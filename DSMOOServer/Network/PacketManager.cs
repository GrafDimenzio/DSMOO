using DSMOOFramework.Analyzer;
using DSMOOFramework.Managers;

namespace DSMOOServer.Network;

public class PacketManager(Analyzer analyzer) : Manager
{
    private readonly Dictionary<Type, PacketAttribute> _packetAttributes = [];
    private readonly Dictionary<short, Type> _packetIdMap = [];
    
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    public void AddPacket(PacketAttribute attribute, Type packetType)
    {
        _packetAttributes[packetType] = attribute;
        _packetIdMap[attribute.Id] = packetType;
    }
    
    public short GetPacketId(Type packetType) => _packetAttributes[packetType].Id;
    public PacketType GetPacketType(Type packetType) => (PacketType)_packetAttributes[packetType].Id;
    public Type GetPacketType(short packetType) => _packetIdMap[packetType];
    public Type GetPacketType(PacketType packetType) => _packetIdMap[(short)packetType];

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<IPacket>()) return;
        var packetAttribute = args.GetAttribute<PacketAttribute>();
        if (packetAttribute == null) return;
        AddPacket(packetAttribute, args.Type);
    }
}