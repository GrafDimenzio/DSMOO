namespace DSMOOServer.API.Stage.Map;

public class MapLocation
{
    public string Kingdom { get; set; } = "";

    public double X { get; set; }
    public double Y { get; set; }

    public int Number { get; set; }
    public int Letter { get; set; }

    public bool SubArea { get; set; }

    public string GetLetter()
    {
        switch (Letter)
        {
            case 1:
                return "A";
            case 2:
                return "B";
            case 3:
                return "C";
            case 4:
                return "D";
            case 5:
                return "E";
            default:
                return "??";
        }
    }

    public string GetCell()
    {
        return GetLetter() + Number;
    }
}