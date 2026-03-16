using DSMOOFramework.Events;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events.Args;

namespace DSMOOServer.API.Events;

public class EventManager(ILogger logger) : Manager
{
    public readonly EventReactor<DummySendPacketEventArgs> OnDummySendPacket = new(logger);
    public readonly EventReactor<GameEventArgs> OnGameEnd = new(logger);
    public readonly EventReactor<GameEventArgs> OnGameStart = new(logger);
    public readonly EventReactor<GetHintEventArgs> OnGetHint = new(logger);
    public readonly EventReactor<PacketReceivedEventArgs> OnPacketReceived = new(logger);
    public readonly EventReactor<PlayerActionEventArgs> OnPlayerAction = new(logger);
    public readonly EventReactor<PlayerAddComponentsEventArgs> OnPlayerAddComponents = new(logger);
    public readonly EventReactor<PlayerCaptureEventArgs> OnPlayerCapture = new(logger);
    public readonly EventReactor<PlayerChangeCostumeEventArgs> OnPlayerChangeCostume = new(logger);
    public readonly EventReactor<PlayerChangeStageEventArgs> OnPlayerChangeStage = new(logger);
    public readonly EventReactor<PlayerCollectMoonEventArgs> OnPlayerCollectMoon = new(logger);
    public readonly EventReactor<PlayerJoinedEventArgs> OnPlayerJoined = new(logger);
    public readonly EventReactor<PlayerDisconnectEventArg> OnPlayerDisconnect = new(logger);
    public readonly EventReactor<PlayerGameEventArgs> OnPlayerJoinGame = new(logger);
    public readonly EventReactor<PlayerGameEventArgs> OnPlayerLeaveGame = new(logger);
    public readonly EventReactor<PlayerLoadedSaveEventArgs> OnPlayerLoadedSave = new(logger);
    public readonly EventReactor<PlayerPreJoinEventArgs> OnPlayerPreJoin = new(logger);
    public readonly EventReactor<PlayerStateEventArgs> OnPlayerState = new(logger);
    public readonly EventReactor<PlayerSwitch2dStateEventArgs> OnPlayerSwitch2dState = new(logger);
    public readonly EventReactor<GameEventArgs> OnPreGameStart = new(logger);
    public readonly EventReactor<SendHintEventArgs> OnSendHint = new(logger);
    public readonly EventReactor<SendPlayerInitPacketEventArgs> OnSendPlayerInitPacket = new(logger);
    public readonly EventReactor<WaitingGameEventArgs> OnWaitingPlayersReleased = new(logger);
}