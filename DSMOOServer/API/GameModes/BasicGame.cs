using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.GameModes.Hints;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public abstract class BasicGame : IGame, IInject, IDisposable
{
    protected readonly Random _random = new();
    protected Task? _hintTask;
    private string? _startingStage;
    private string? _waitingStage;

    [Inject] public ILogger Logger;
    [Inject] public GameModeManager GameModeManager { get; set; }

    [Inject] public EventManager EventManager { get; set; }
    public string[] Arguments { get; private set; } = [];

    public void Dispose()
    {
        EventManager.OnPlayerChangeStage.Unsubscribe(OnChangeStage);
    }

    public bool IsRunning { get; protected set; }
    public abstract string DisplayName { get; }
    public IPlayer[] Players { get; private set; } = [];
    public StagePreset StagePreset { get; private set; }
    public HintPreset HintPreset { get; private set; }

    public void StartGame(IPlayer[] playingPlayers, StagePreset stagePreset, HintPreset hintPreset, string[] arguments)
    {
        IsRunning = true;
        Players = playingPlayers;
        StagePreset = stagePreset;
        HintPreset = hintPreset;
        Arguments = arguments;
        EventManager.OnPreGameStart.RaiseEvent(new GameEventArgs { Game = this });
        OnGameStart();
        EventManager.OnGameStart.RaiseEvent(new GameEventArgs { Game = this });
    }

    public void EndGame()
    {
        OnGameEnd();
        EventManager.OnGameEnd.RaiseEvent(new GameEventArgs { Game = this });
        Players = [];
        IsRunning = false;
    }

    public void AddPlayerToGame(IPlayer player)
    {
        Players = Players.Concat([player]).ToArray();
        OnPlayerJoinGame(player);
        EventManager.OnPlayerJoinGame.RaiseEvent(new PlayerGameEventArgs { Player = player, Game = this });
    }

    public void RemovePlayerFromGame(IPlayer player)
    {
        if (!Players.Contains(player))
            return;

        Players = Players.Except([player]).ToArray();
        OnPlayerLeaveGame(player);
        EventManager.OnPlayerLeaveGame.RaiseEvent(new PlayerGameEventArgs { Player = player, Game = this });
    }

    public void AfterInject()
    {
        EventManager.OnPlayerChangeStage.Subscribe(OnChangeStage);
    }

    protected virtual void OnChangeStage(PlayerChangeStageEventArgs eventArgs)
    {
        if (StagePreset.AllowAll || !IsRunning || !Players.Contains(eventArgs.Player) ||
            IsStageAllowed(eventArgs.NewStage))
            return;
        eventArgs.SendBack = true;
        //Just a fail safe
        if (!IsStageAllowed(eventArgs.SendBackStage))
            eventArgs.SendBackStage = GetStartingStage();
    }

    protected virtual void OnGameStart()
    {
        if (HintPreset.Hints.Length > 0)
            _hintTask = Task.Run(HintTask);
        foreach (var player in Players) player.ChangeStage(GetStartingStage());
    }

    protected virtual void OnGameEnd()
    {
    }

    protected virtual void OnPlayerJoinGame(IPlayer player)
    {
        player.ChangeStage(GetStartingStage());
    }

    protected virtual void OnPlayerLeaveGame(IPlayer player)
    {
    }

    protected virtual IPlayer[] PlayersToHint()
    {
        return Players;
    }

    protected virtual void SendHint(HintData hintData)
    {
        foreach (var player in Players)
            //if (player == hintData.Player)
            //continue;
            GameModeManager.SendHintToPlayer(player, hintData);
    }

    protected async Task HintTask()
    {
        var current = 0;
        var time = 0;

        while (current < HintPreset.Hints.Length)
        {
            var hint = HintPreset.Hints[current];
            var timeToWait = hint.Time - time;
            await Task.Delay(timeToWait * 1000);
            if (!IsRunning)
                break;
            time += timeToWait;

            var hintTypes = new HashSet<string>();
            var hintIndex = HintPreset.UpdateOldHintOnNewOnes ? 0 : current;
            while (hintIndex <= current)
            {
                var currentHint = HintPreset.Hints[hintIndex];
                hintTypes.Add(currentHint.HintType);
                hintIndex++;
            }

            foreach (var player in PlayersToHint())
            {
                var hintData = GameModeManager.GetHint(hintTypes.ToArray(), player, this);
                SendHint(hintData);
            }

            current++;
        }
    }

    public string GetStartingStage()
    {
        if (!string.IsNullOrEmpty(_startingStage))
            return _startingStage;
        var stages = StagePreset.StartingStages;
        var stage = stages[_random.Next(stages.Length)];
        if (StagePreset.AllOnSameStartingStage)
            _startingStage = stage;
        return stage;
    }

    public string GetWaitingStage()
    {
        if (!string.IsNullOrEmpty(_waitingStage))
            return _waitingStage;
        var stages = StagePreset.WaitingStages;
        var stage = stages[_random.Next(stages.Length)];
        if (StagePreset.AllOnSameWaitingStage)
            _waitingStage = stage;
        return stage;
    }

    public bool IsStageAllowed(string stage)
    {
        return StagePreset.StartingStages.Contains(stage) || StagePreset.AllowedStages.Contains(stage);
    }
}