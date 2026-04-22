using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOServer.API.Player;
using DSMOOServer.Connection;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.GameModes;

[Game(Names = ["sardines", "sardine"])]
public class Sardines(PlayerManager playerManager, ILogger logger) : WaitingGame
{
    public override int TeamSize => Players.Length - 1;

    public override string DisplayName { get; } = "Sardines";

    protected override void OnGameStart()
    {
        if (Arguments.Length > 0 && int.TryParse(Arguments[0], out var waitingTime)) WaitingTime = waitingTime * 1000;
        if (Arguments.Length > 1)
        {
            var search = playerManager.SearchForPlayers(Arguments[1..]);
            if (search.Players.Count > 0)
            {
                StartTeamPlayers = [search.Players[0]];
                var players = Players.ToList();
                players.Remove(search.Players[0]);
                WaitingTeamPlayers = players.ToArray();
            }
        }
        base.OnGameStart();
    }

    protected override void StartPlayers()
    {
        foreach (var player in StartTeamPlayers) player.ChangeGameState(GameMode.Sardines, false);

        base.StartPlayers();
    }

    protected override void StartWaitingPlayers()
    {
        foreach (var player in WaitingTeamPlayers) player.ChangeGameState(GameMode.Sardines, false);

        base.StartWaitingPlayers();
    }

    protected override void SpawnWaitingPlayers()
    {
        foreach (var player in StartTeamPlayers) player.ChangeGameState(GameMode.Sardines, true);

        base.SpawnWaitingPlayers();
    }

    protected override IPlayer[] PlayersToHint()
    {
        return [StartTeamPlayers.First()];
    }
}