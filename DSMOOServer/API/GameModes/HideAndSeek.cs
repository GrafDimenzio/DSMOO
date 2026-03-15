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

    protected override void OnGameStart()
    {
        if (Arguments.Length > 0 && int.TryParse(Arguments[0], out var waitingTime)) WaitingTime = waitingTime * 1000;
        if (Arguments.Length > 1 && int.TryParse(Arguments[1], out var teamSize)) TeamSize = teamSize;
        base.OnGameStart();
    }

    protected override void OnGameEnd()
    {
        foreach (var player in StartTeamPlayers)
        {
            player.ChangeGameState(GameMode.HideAndSeek, true);
        }
        
        base.OnGameEnd();
    }

    protected override void OnPlayerJoinGame(IPlayer player)
    {
        player.ChangeGameState(GameMode.HideAndSeek, Waiting);
        
        base.OnPlayerJoinGame(player);
    }

    protected override void StartPlayers()
    {
        foreach (var player in StartTeamPlayers)
        {
            player.ChangeGameState(GameMode.HideAndSeek, true);
        }
        
        base.StartPlayers();
    }

    protected override void StartWaitingPlayers()
    {
        foreach (var player in WaitingTeamPlayers)
        {
            player.ChangeGameState(GameMode.HideAndSeek, true);
        }
        
        base.StartWaitingPlayers();
    }

    protected override void SpawnWaitingPlayers()
    {
        foreach (var player in StartTeamPlayers)
        {
            player.ChangeGameState(GameMode.HideAndSeek, false);
        }

        base.SpawnWaitingPlayers();
    }

    protected override IPlayer[] PlayersToHint()
    {
        return StartTeamPlayers.Where(x => !x.IsIt).ToArray();
    }
}