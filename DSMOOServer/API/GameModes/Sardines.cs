using DSMOOFramework.Controller;
using DSMOOServer.API.Player;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

[Game(Names = ["sardines", "sardine"])]
public class Sardines : WaitingGame
{
    [Inject] public Server Server { get; set; }

    public override int TeamSize => Players.Length - 1;

    public override string DisplayName { get; } = "Sardines";

    public override void OnGameStart()
    {
        if (Arguments.Length > 0 && int.TryParse(Arguments[0], out var waitingTime))
        {
            WaitingTime = waitingTime * 1000;
        }
        base.OnGameStart();
    }

    protected override void StartPlayers()
    {
        var packet = new TagPacket
        {
            IsIt = false,
            GameMode = GameMode.Sardines,
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
            IsIt = false,
            GameMode = GameMode.Sardines,
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
            IsIt = true,
            GameMode = GameMode.Sardines,
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