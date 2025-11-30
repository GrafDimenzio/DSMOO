namespace DSMOOFramework.Plugins;

public class PluginAttribute : Attribute
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public string Author { get; set; } = "";
    public string Repository { get; set; } = "";
}