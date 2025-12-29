using System.Numerics;
using DSMOOServer.API.Stage;

namespace DSMOOServer.API.Map;

public class MapLocation
{
    public MapLocation(Vector3 position, string stageName)
    {
        Kingdom = Stages.Alias2Stage[Stages.Stage2Alias[stageName]];
        var infoTable = MapInfo.AllKingdoms.FirstOrDefault(x => x.MainStageName == Kingdom);
        if (infoTable == null)
            return;
        if (stageName == Kingdom)
        {
            X = position.X;
            Y = position.Z;
        }
        else if (infoTable.SubAreas.Any(x => x.SubAreaName == stageName))
        {
            var sub = infoTable.SubAreas.First(x => x.SubAreaName == stageName);
            X = sub.Position.X;
            Y = sub.Position.Y;
            SubArea = true;
        }
        else
        {
            X = 2048;
            Y = 2048;
        }

        if (infoTable.Rotation != 0)
        {
            var angle = Math.PI * -infoTable.Rotation;
            var oldX = X;
            var oldY = Y;

            Y = oldY * Math.Cos(angle) - oldX * Math.Sin(angle);
            X = oldY * Math.Sin(angle) + oldX * Math.Cos(angle);
        }

        X += infoTable.Offset.X;
        Y += infoTable.Offset.Y;

        X /= infoTable.Scale;
        Y /= infoTable.Scale;

        Number = (int)((X - 65) / 390) + 1;
        Letter = (int)((Y - 65) / 390) + 1;
    }

    public string Kingdom { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    public int Number { get; set; }
    public int Letter { get; set; }

    public bool SubArea { get; set; }

    public string GetCell()
    {
        switch (Letter)
        {
            case 1:
                return "A" + Number;
            case 2:
                return "B" + Number;
            case 3:
                return "C" + Number;
            case 4:
                return "D" + Number;
            case 5:
                return "E" + Number;
            default:
                return "??";
        }
    }
}