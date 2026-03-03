using DSMOOFramework.Controller;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.GameModes.Hints;
using DSMOOServer.API.Player;

namespace DSMOOServer.API.GameModes;

public abstract class BasicGame : IGame, IInject, IDisposable
{
    private Task? _hintTask;
    private readonly Random _random = new();
    private string? _startingStage;
    private string? _waitingStage;

    [Inject] public GameModeManager GameModeManager { get; set; }

    [Inject] public EventManager EventManager { get; set; }

    public abstract string Name { get; }
    public IPlayer[] Players { get; private set; } = [];
    public StageConfig StageConfig { get; private set; }
    public HintConfig HintConfig { get; private set; }

    public void StartGame(IPlayer[] playingPlayers, StageConfig stageConfig, HintConfig hintConfig, string[] arguments)
    {
        Players = playingPlayers;
        StageConfig = stageConfig;
        HintConfig = hintConfig;
        if (HintConfig.Hints.Length > 0)
            _hintTask = Task.Run(HintTask);
        EventManager.OnPreGameStart.RaiseEvent(new GameEventArgs { Game = this });
        OnGameStart();
        EventManager.OnGameStart.RaiseEvent(new GameEventArgs { Game = this });
    }

    public void EndGame()
    {
        _hintTask?.Dispose();
        OnGameEnd();
        EventManager.OnGameEnd.RaiseEvent(new GameEventArgs { Game = this });
        Players = [];
    }

    public void AfterInject()
    {
        EventManager.OnPlayerChangeStage.Subscribe(OnChangeStage);
    }

    protected virtual void OnChangeStage(PlayerChangeStageEventArgs eventArgs)
    {
        if (!Players.Contains(eventArgs.Player) || IsStageAllowed(eventArgs.NewStage))
            return;
        eventArgs.SendBack = true;
    }

    public virtual void OnGameStart()
    {
        foreach (var player in Players) player.ChangeStage(GetStartingStage());
    }

    public virtual void OnGameEnd()
    {
    }

    public virtual IPlayer[] PlayersToHint()
    {
        return Players;
    }

    public virtual void SendHint(HintData hintData)
    {
    }

    private async Task HintTask()
    {
        var current = 0;
        var time = 0;

        while (current < HintConfig.Hints.Length)
        {
            var hint = HintConfig.Hints[current];
            var timeToWait = hint.Time - time;
            await Task.Delay(timeToWait * 1000);
            time += timeToWait;
            foreach (var player in PlayersToHint())
            {
                var hintIndex = HintConfig.UpdateOldHintOnNewOnes ? 0 : current;
                while (hintIndex <= current)
                {
                    var hintData = GameModeManager.GetHint(HintConfig.Hints[hintIndex].HintType, player, this);
                    SendHint(hintData);
                    hintIndex++;
                }
            }

            current++;
        }
    }

    public string GetStartingStage()
    {
        if (!string.IsNullOrEmpty(_startingStage))
            return _startingStage;
        var stages = StageConfig.StartingStage;
        var stage = stages[_random.Next(stages.Length)];
        if (StageConfig.AllOnSameStartingStage)
            _startingStage = stage;
        return stage;
    }

    public string GetWaitingStage()
    {
        if (!string.IsNullOrEmpty(_waitingStage))
            return _waitingStage;
        var stages = StageConfig.WaitingStage;
        var stage = stages[_random.Next(stages.Length)];
        if (StageConfig.AllOnSameWaitingStage)
            _startingStage = stage;
        return stage;
    }

    public bool IsStageAllowed(string stage)
    {
        return StageConfig.StartingStage.Contains(stage) || StageConfig.AllowedStages.Contains(stage);
    }

    public void Dispose()
    {
        _hintTask?.Dispose();
        EventManager.OnPlayerChangeStage.Unsubscribe(OnChangeStage);
    }
}