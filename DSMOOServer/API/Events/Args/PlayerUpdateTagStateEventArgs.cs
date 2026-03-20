using DSMOOFramework.Events;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Events.Args;

public class PlayerUpdateTagStateEventArgs : IEventArg
{
    public bool IsIt { get; init; }
    public GameMode GameMode { get; init; }

    public bool PreviousIsIt { get; init; }
    public GameMode PreviousGameMode { get; init; }
}