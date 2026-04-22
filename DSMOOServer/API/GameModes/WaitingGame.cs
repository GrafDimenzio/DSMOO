using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public abstract class WaitingGame : BasicGame
{
    protected Task _waitingTask;
    
    public Guid[] WaitingPlayersBackup { get; protected set; }

    public IPlayer[] WaitingTeamPlayers { get; protected set; }
    public IPlayer[] StartTeamPlayers { get; protected set; }

    public virtual int WaitingTime { get; protected set; } = 15000;
    public virtual int TeamSize { get; protected set; } = 1;

    public bool Waiting { get; protected set; }

    protected override void OnGameStart()
    {
        var possiblePlayers = Players.ToList();
        var waitingTeam = new List<IPlayer>();
        while (waitingTeam.Count < TeamSize)
        {
            var player = possiblePlayers[_random.Next(0, possiblePlayers.Count)];
            possiblePlayers.Remove(player);
            waitingTeam.Add(player);
        }

        WaitingTeamPlayers = waitingTeam.ToArray();
        WaitingPlayersBackup = waitingTeam.Select(x => x.Id).ToArray();
        StartTeamPlayers = possiblePlayers.ToArray();
        Waiting = true;

        StartPlayers();
        StartWaitingPlayers();
        _waitingTask = Task.Run(WaitForStart);
    }

    protected override void OnGameEnd()
    {
        WaitingTeamPlayers = [];
        StartTeamPlayers = [];
    }

    protected override void OnPlayerJoinGame(IPlayer player)
    {
        if (WaitingPlayersBackup.Contains(player.Id))
        {
            WaitingTeamPlayers = WaitingTeamPlayers.Where(x => x.Id != player.Id).Concat([player]).ToArray();
            player.NextStageOverride = Waiting ? GetWaitingStage() : GetStartingStage();
            return;
        }
        StartTeamPlayers = StartTeamPlayers.Concat([player]).ToArray();
        base.OnPlayerJoinGame(player);
    }

    protected override void OnPlayerLeaveGame(IPlayer player)
    {
        StartTeamPlayers = StartTeamPlayers.Except([player]).ToArray();
        WaitingTeamPlayers = WaitingTeamPlayers.Except([player]).ToArray();
        base.OnPlayerLeaveGame(player);
    }

    protected virtual async Task WaitForStart()
    {
        await Task.Delay(WaitingTime);
        if (!IsRunning)
            return;
        Waiting = false;
        SpawnWaitingPlayers();
        if (HintPreset.Hints.Length > 0)
            _hintTask = Task.Run(HintTask);
    }

    protected override IPlayer[] PlayersToHint()
    {
        return StartTeamPlayers;
    }

    protected override void OnChangeStage(PlayerChangeStageEventArgs eventArgs)
    {
        if (IsRunning && Waiting && WaitingTeamPlayers.Contains(eventArgs.Player))
        {
            if (StagePreset.WaitingStages.Contains(eventArgs.NewStage))
                return;

            eventArgs.SendBack = true;
            return;
        }

        base.OnChangeStage(eventArgs);
    }

    protected virtual void StartPlayers()
    {
        foreach (var player in StartTeamPlayers) player.ChangeStage(GetStartingStage());
    }

    protected virtual void StartWaitingPlayers()
    {
        foreach (var player in WaitingTeamPlayers) player.ChangeStage(GetWaitingStage());
    }

    protected virtual void SpawnWaitingPlayers()
    {
        EventManager.OnWaitingPlayersReleased.RaiseEvent(new WaitingGameEventArgs { Game = this });
        foreach (var player in WaitingTeamPlayers) player.ChangeStage(GetStartingStage());
    }
}