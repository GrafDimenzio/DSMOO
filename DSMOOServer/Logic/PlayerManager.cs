using System.Net;
using System.Numerics;
using System.Text;
using DSMOOFramework.Config;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API;
using DSMOOServer.API.Enum;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Map;
using DSMOOServer.API.Player;
using DSMOOServer.Helper;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Logic;

public class PlayerManager(EventManager eventManager, ILogger logger, ConfigHolder<ServerMainConfig> holder) : Manager
{
    public readonly List<IPlayer> Players = [];

    public List<IPlayer> RealPlayers =>
        Players.Where(x => !x.IsDummy && x is Player { Client.Socket.Connected: true }).ToList();

    public int PlayerCount => RealPlayers.Select(x => x.Id).Distinct().Count();

    public PlayerSearch SearchForPlayers(string[] args)
    {
        var search = new PlayerSearch();

        if (args.Length == 0)
            return search;

        switch (args[0])
        {
            case "*":
                search.Players = RealPlayers;
                return search;

            case "!*":
                search.IsInverted = true;
                search.Players = RealPlayers;
                args = args[1..];
                break;
        }

        foreach (var argument in args)
        {
            if (string.IsNullOrWhiteSpace(argument)) continue;

            var possiblePlayers = RealPlayers.Where(player =>
                player.Name.ToLower().StartsWith(argument.ToLower()) ||
                (Guid.TryParse(argument, out var guid) && guid == player.Id) ||
                (IPAddress.TryParse(argument, out var ip) && ip.Equals(player.Ip))).ToList();


            switch (possiblePlayers.Count)
            {
                case 0:
                    search.FailedToFind.Add(argument);
                    continue;

                case 1:
                    HandlePlayer(possiblePlayers[0]);
                    continue;

                case > 1:
                    var exactName = possiblePlayers.FirstOrDefault(x => x.Name == argument);
                    if (exactName != null)
                    {
                        HandlePlayer(exactName);
                        continue;
                    }

                    search.MultipleMatches.Add((argument, possiblePlayers.ToArray()));
                    continue;
            }
        }

        return search;

        void HandlePlayer(IPlayer player)
        {
            if (search.IsInverted)
                search.Players.Remove(player);
            else
                search.Players.Add(player);
        }
    }

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        var player = args.Sender.Player;
        switch (args.Packet)
        {
            case GamePacket gamePacket:
                //On a Stage change remove all the data from the PlayerPacket
                if (player.Stage != gamePacket.Stage)
                {
                    player.Position = Vector3.Zero;
                    player.Rotation = Quaternion.Identity;
                    player.AnimationBlendWeights = [0];
                    player.Act = 0;
                    player.SubAct = 0;
                }

                if (player.Stage != gamePacket.Stage || player.Scenario != gamePacket.ScenarioNum)
                {
                    var warp = MapInfo.GetConnection(player.Stage, gamePacket.Stage, false);
                    var bytes = Encoding.UTF8.GetBytes(warp);
                    if (bytes.Length > ChangeStagePacket.IdSize)
                        warp = "";

                    var changeStageArgs = new PlayerChangeStageEventArgs
                    {
                        Player = player,
                        NewStage = gamePacket.Stage,
                        PreviousStage = player.Stage,
                        PreviousScenario = player.Scenario,
                        NewScenario = gamePacket.ScenarioNum,

                        SendBackScenario = player.Scenario > sbyte.MaxValue ? (sbyte)-1 : (sbyte)player.Scenario,
                        SendBackWarp = warp,
                        SendBackStage = player.Stage
                    };
                    eventManager.OnPlayerChangeStage.RaiseEvent(changeStageArgs);
                    if (changeStageArgs.SendBack && !string.IsNullOrWhiteSpace(changeStageArgs.SendBackStage))
                        player.ChangeStage(changeStageArgs.SendBackStage, changeStageArgs.SendBackWarp,
                            changeStageArgs.SendBackScenario, 0, 1000);
                }

                if (player.Is2d != gamePacket.Is2d)
                    eventManager.OnPlayerSwitch2dState.RaiseEvent(new PlayerSwitch2dStateEventArgs
                    {
                        Player = player,
                        IsNow2d = gamePacket.Is2d
                    });

                player.Scenario = gamePacket.ScenarioNum;
                player.Is2d = gamePacket.Is2d;
                player.Stage = gamePacket.Stage;
                break;

            case TagPacket tagPacket:
                var updateTime = tagPacket.UpdateType is TagPacket.TagUpdate.Time or TagPacket.TagUpdate.Both
                    or TagPacket.TagUpdate.All;
                var updateState = tagPacket.UpdateType is TagPacket.TagUpdate.State or TagPacket.TagUpdate.Both
                    or TagPacket.TagUpdate.All;
                if (updateState)
                {
                    player.CurrentGameMode = tagPacket.GameMode;
                    player.IsIt = tagPacket.IsIt;
                }

                if (updateTime)
                    player.Time = new Time(tagPacket.Minutes, tagPacket.Seconds, DateTime.Now);
                break;

            case CapturePacket capturePacket:
                if (player.Capture != capturePacket.ModelName)
                    eventManager.OnPlayerCapture.RaiseEvent(new PlayerCaptureEventArgs
                    {
                        Player = player,
                        Capture = capturePacket.ModelName
                    });
                player.Capture = capturePacket.ModelName;
                break;

            case CostumePacket costumePacket:
                if (player.Costume.CapName != costumePacket.CapName ||
                    player.Costume.BodyName != costumePacket.BodyName)
                    eventManager.OnPlayerChangeCostume.RaiseEvent(new PlayerChangeCostumeEventArgs
                    {
                        Player = player,
                        OldCostume = player.Costume,
                        NewCostume = costumePacket
                    });
                player.Costume = costumePacket;

                if (!player.IsSaveLoaded)
                {
                    player.IsSaveLoaded = true;
                    eventManager.OnPlayerLoadedSave.RaiseEvent(new PlayerLoadedSaveEventArgs(player));
                }

                break;

            case PlayerPacket playerPacket:
                player.Position = playerPacket.Position;
                player.Rotation = playerPacket.Rotation;

                var action = APIConstants.PlayerActions.GetValueOrDefault(playerPacket.Act, PlayerAction.None);

                if (player.LastPlayerAction != action)
                {
                    eventManager.OnPlayerAction.RaiseEvent(new PlayerActionEventArgs
                    {
                        Player = player,
                        Action = action
                    });
                    player.LastPlayerAction = action;
                }

                player.AnimationBlendWeights = playerPacket.AnimationBlendWeights;
                player.Act = playerPacket.Act;
                player.SubAct = playerPacket.SubAct;

                var ev = new PlayerStateEventArgs
                {
                    Player = player,
                    Packet = playerPacket
                };
                eventManager.OnPlayerState.RaiseEvent(ev);
                if (!ev.Packet.Equals(playerPacket) || ev.Invisible)
                {
                    var copy = playerPacket;
                    if (ev.Invisible)
                        copy.Position = Vector3.UnitY * -10000;
                    args.ReplacePacket = copy;
                }

                foreach (var packetPair in ev.SpecificPackets)
                    args.SpecificReplacePackets[packetPair.Key] = packetPair.Value;

                foreach (var id in ev.SpecificInvisible)
                {
                    if (args.SpecificReplacePackets.TryGetValue(id, out var packet))
                    {
                        //Nothing since TryGetValue already did its thing
                    }
                    else if (args.ReplacePacket != null)
                    {
                        packet = args.ReplacePacket;
                    }
                    else
                    {
                        packet = args.Packet;
                    }

                    if (packet is not PlayerPacket invisPacket)
                        continue;

                    invisPacket.Position = Vector3.UnitY * -10000;
                    args.SpecificReplacePackets[id] = invisPacket;
                }

                break;
        }
    }

    public class PlayerSearch
    {
        public bool IsInverted { get; set; }
        public List<IPlayer> Players { get; set; } = [];
        public List<string> FailedToFind { get; set; } = [];
        public List<(string, IPlayer[])> MultipleMatches { get; set; } = [];
    }
}