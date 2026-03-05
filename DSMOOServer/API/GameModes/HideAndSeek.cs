using DSMOOFramework.Controller;
using DSMOOServer.Connection;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

[Game(Names = ["HideAndSeek", "Hide&Seek"])]
public class HideAndSeek : WaitingGame
{
    [Inject] public Server Server { get; set; }

    public override string DisplayName { get; } = "Hide & Seek";

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
}