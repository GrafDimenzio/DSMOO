namespace DSMOOFramework.Config;

public interface IConfigHolder
{
    public IConfig ConfigObject { get; set; }

    public void SaveConfig();

    public void LoadConfig();
}