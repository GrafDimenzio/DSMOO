using System.Numerics;
using System.Text;
using System.Text.Json;
using DSMOOFramework.Commands;
using DSMOOServer.API.Serialized;
using DSMOOServer.API.Stage;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Commands;

[Command(
    CommandName = "adjust",
    Aliases = [],
    Description = "",
    Parameters = []
)]
public class AdjustJson(StageManager manager, PlayerManager playerManager) : Command
{
    public override CommandResult Execute(string command, string[] args)
    {
        var stages = new List<StageInfo>();
        var player = playerManager.Players.First();
        foreach (var stage in manager.Stages)
        {
            var warps = new List<WarpInfo>();

            foreach (var currentWarp in stage.Warps)
            {
                Console.WriteLine($"\n\nTesting {stage.StageName}-{currentWarp.Name}");
                var tooLong = false;
                if (Encoding.UTF8.GetBytes(currentWarp.Name).Length > Constants.WarpIdSize)
                {
                    Console.WriteLine("Warp is Too Long!");
                    tooLong = true;
                    player.ChangeStage(stage.StageName);
                }
                else
                {
                    player.ChangeStage(stage.StageName, currentWarp.Name);
                }
                Console.WriteLine("Press a key once loaded in");
                var response = Console.ReadKey();
                if (response.Key == ConsoleKey.F1)
                    goto END;
                var position = Vector3.Zero;
                if (!tooLong)
                {
                    position = new Vector3((float)Math.Round(player.Position.X), (float)Math.Round(player.Position.Y),
                        (float)Math.Round(player.Position.Z));
                }
                Console.WriteLine($"Using Position {position}");
                var connStage = currentWarp.ConnectedStage;
                if (string.IsNullOrWhiteSpace(connStage))
                {
                    Console.WriteLine("Please Press any Key once you are in the connected Stage!");
                    Console.ReadKey();
                    connStage = player.Stage;
                }
                
                warps.Add(new WarpInfo()
                {
                    Name = currentWarp.Name,
                    Position = position,
                    ActiveScenarios = currentWarp.ActiveScenarios,
                    ConnectedStage = connStage
                });
            }
            
            stages.Add(new StageInfo()
            {
                StageName = stage.StageName,
                Warps = warps.ToArray(),
                Alias = stage.Alias,
            });
        }
        END:
        
        var json = JsonSerializer.Serialize(stages);
        File.WriteAllText("/home/dimenzio/Applications/stages.json", json);
        
        return $"DONE!";
    }
}