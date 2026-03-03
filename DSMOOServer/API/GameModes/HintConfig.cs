namespace DSMOOServer.API.GameModes;

public class HintConfig
{
    public string Name { get; set; } = "";
    public bool UpdateOldHintOnNewOnes { get; set; } = true;

    public Hint[] Hints { get; set; } = [];

    public class Hint
    {
        public int Time { get; set; } = 0;
        public string HintType { get; set; } = "";
    }
}