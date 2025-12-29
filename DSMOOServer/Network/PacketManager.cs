using DSMOOFramework.Analyzer;
using DSMOOFramework.Managers;

namespace DSMOOServer.Network;

public class PacketManager(Analyzer analyzer) : Manager
{
    private readonly Dictionary<Type, short> _packetAttributes = [];
    private readonly Dictionary<short, Type> _packetIdMap = [];
    
    public override void Initialize()
    {
        analyzer.OnAnalyze.Subscribe(OnAnalyze);
    }

    public void AddPacket(short id, Type packetType)
    {
        _packetAttributes[packetType] = id;
        _packetIdMap[id] = packetType;
    }
    
    public short GetPacketId(Type packetType) => _packetAttributes[packetType];
    public PacketType GetPacketType(Type packetType) => (PacketType)_packetAttributes[packetType];
    public Type? GetPacketType(short packetType) => _packetIdMap.ContainsKey(packetType) ? _packetIdMap[packetType] : null;
    public Type GetPacketType(PacketType packetType) => _packetIdMap[(short)packetType];

    private void OnAnalyze(AnalyzeEventArgs args)
    {
        if (!args.Is<IPacket>()) return;
        var packetAttribute = args.GetAttribute<PacketAttribute>();
        if (packetAttribute == null) return;
        AddPacket(packetAttribute.Id, args.Type);
    }
}