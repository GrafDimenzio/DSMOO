using System.Net;
using System.Numerics;
using DSMOOServer.Helper;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Player;

public interface IPlayer
{
    public IPAddress? Ip { get; }
    public Guid Id { get; }
    public bool IsDummy { get; }
    public string Name { get; }

    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    public ushort Act { get; }
    public ushort SubAct { get; }
    public float[] AnimationBlendWeights { get; }

    public CostumePacket Costume { get; }
    public string Capture { get; }
    
    public bool Is2d { get; }
    public byte Scenario { get; }
    public string Stage { get; }
    
    public GameMode CurrentGameMode { get; }
    public bool IsIt { get; }
    public Time Time { get; }
    
    public bool IsSaveLoaded { get; }
    
    public bool IsBanned { get; }

    
    public void Disconnect();

    public void Crash(bool ban);
    
    public Task SendShine(int id);

    public Task ChangeStage(string stage, string warp, sbyte scenario = -1, byte subScenarioType = 0, int delay = 0);

    public Task Send<T>(T packet, Guid? sender) where T : struct, IPacket;
}