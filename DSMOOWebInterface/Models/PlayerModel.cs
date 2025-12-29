using System.Numerics;
using DSMOOServer.API.Player;
using DSMOOServer.Network.Packets;

namespace DSMOOWebInterface.Models;

public class PlayerModel(IPlayer player)
{
    public string Name { get; set; } = player.Name;
    public Guid Id { get; set; } = player.Id;
    public string Capture { get; set; } = player.Capture == string.Empty ? "None" : player.Capture;
    public bool Is2D { get; set; } = player.Is2d;
    public bool IsIt { get; set; } = player.IsIt;
    public Vector3 Position { get; set; } = player.Position;
    public Quaternion Rotation { get; set; } = player.Rotation;
    public string Cap { get; set; } = player.Costume.CapName;
    public string Body { get; set; } = player.Costume.BodyName;
    public string Stage { get; set; } = player.Stage;
    public GameMode GameMode { get; set; } = player.CurrentGameMode;
}