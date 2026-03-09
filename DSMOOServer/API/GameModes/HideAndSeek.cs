using DSMOOFramework.Controller;
using DSMOOServer.API.Player;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

[Game(Names = ["HideAndSeek", "Hide&Seek"])]
public class HideAndSeek : WaitingGame
{
    [Inject] public Server Server { get; set; }

    public override string DisplayName { get; } = "Hide & Seek";

    public override void OnGameStart()
    {
        if (Arguments.Length > 0 && int.TryParse(Arguments[0], out var waitingTime)) WaitingTime = waitingTime * 1000;
        if (Arguments.Length > 1 && int.TryParse(Arguments[1], out var teamSize)) TeamSize = teamSize;
        base.OnGameStart();
    }

    protected override void StartPlayers()
    {
        var packet = new TagPacket
        {
            IsIt = true,
            GameMode = GameMode.HideAndSeek,
            UpdateType = TagPacket.TagUpdate.State
        };
        foreach (var player in StartTeamPlayers)
        {
            Server.Broadcast(packet, player.Id);
            player.Send(packet, null);
            player.ChangeStage(GetStartingStage());
        }
    }

    protected override void StartWaitingPlayers()
    {
        var packet = new TagPacket
        {
            IsIt = true,
            GameMode = GameMode.HideAndSeek,
            UpdateType = TagPacket.TagUpdate.State
        };
        foreach (var player in WaitingTeamPlayers)
        {
            Server.Broadcast(packet, player.Id);
            player.Send(packet, null);
            player.ChangeStage(GetWaitingStage());
        }
    }

    protected override void SpawnWaitingPlayers()
    {
        var packet = new TagPacket
        {
            IsIt = false,
            GameMode = GameMode.HideAndSeek,
            UpdateType = TagPacket.TagUpdate.State
        };
        foreach (var player in StartTeamPlayers)
        {
            Server.Broadcast(packet, player.Id);
            player.Send(packet, null);
        }

        base.SpawnWaitingPlayers();
    }

    public override IPlayer[] PlayersToHint()
    {
        return StartTeamPlayers.Where(x => !x.IsIt).ToArray();
    }
}