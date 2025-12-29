using System.Numerics;
using DSMOOFramework.Logger;
using DSMOOFramework.Plugins;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.API.Player;
using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;

namespace DSMOOFlip;

[Plugin(
    Name = "Flip",
    Description = "Plugin that adds the Flip Command",
    Author = "Dimenzio",
    Version = "1.0.0",
    Repository = "https://github.com/GrafDimenzio/DSMOO"
)]
public class FlipManager(
    ILogger logger,
    EventManager eventManager,
    PlayerManager playerManager) : Plugin<FlipConfig>
{
    private readonly Quaternion _flip = Quaternion.CreateFromYawPitchRoll(MathF.PI, MathF.PI, 0);
    public HashSet<Guid> Players => Config.Players;

    public FlipOptions FlipOptions
    {
        get => Config.Options;
        set
        {
            Config.Options = value;
            SaveConfig();
        }
    }

    public override void Initialize()
    {
        eventManager.OnPacketReceived.Subscribe(OnPacket);
        logger.Info("Loaded Flip Plugin");
    }

    private void OnPacket(PacketReceivedEventArgs args)
    {
        if (!Config.Enabled) return;
        switch (args.Packet)
        {
            case PlayerPacket playerPacket:
                PlayerPacket? flippedPacket = null;
                if (Config.Players.Contains(args.Sender.Id) && Config.Options.HasFlag(FlipOptions.Other))
                {
                    flippedPacket = FlipPlayer(args.Sender.Player, playerPacket);
                    args.ReplacePacket = FlipPlayer(args.Sender.Player, playerPacket);
                }

                if (Config.Options.HasFlag(FlipOptions.Self) && Config.Players.Count > 0)
                {
                    flippedPacket ??= FlipPlayer(args.Sender.Player, playerPacket);
                    foreach (var player in playerManager.RealPlayers)
                    {
                        if (player.Id == args.Sender.Id) continue;

                        if (Config.Players.Contains(player.Id))
                            args.SpecificReplacePackets[player.Id] = flippedPacket;
                    }
                }


                break;
        }
    }

    private PlayerPacket FlipPlayer(IPlayer player, PlayerPacket playerPacket)
    {
        playerPacket.Position += Vector3.UnitY * MarioSize(player.Is2d);
        playerPacket.Rotation *= _flip;
        return playerPacket;
    }

    private float MarioSize(bool is2d)
    {
        return is2d ? 180 : 160;
    }
}