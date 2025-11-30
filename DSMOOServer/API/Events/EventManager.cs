using DSMOOFramework.Analyzer;
using DSMOOFramework.Events;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events.Args;

namespace DSMOOServer.API.Events;

[Analyze(Priority = 100)]
public class EventManager(ILogger logger) : Manager
{
    public readonly EventReactor<PacketReceivedEventArgs> OnPacketReceived = new(logger);
    public readonly EventReactor<PlayerJoinedEventArgs> OnPlayerJoined = new(logger);
    public readonly EventReactor<PlayerLoadedSaveEventArgs> OnPlayerLoadedSave = new(logger);
    public readonly EventReactor<PlayerChangeStageEventArgs> OnPlayerChangeStage = new(logger);
    public readonly EventReactor<PlayerSwitch2dStateEventArgs> OnPlayerSwitch2dState = new(logger);
    public readonly EventReactor<PlayerCaptureEventArgs> OnPlayerCapture = new(logger);
    public readonly EventReactor<PlayerCollectMoonEventArgs> OnPlayerCollectMoon = new(logger);
    public readonly EventReactor<PlayerChangeCostumeEventArgs> OnPlayerChangeCostume = new(logger);
    public readonly EventReactor<DummySendPacketEventArgs> OnDummySendPacket = new(logger);
}