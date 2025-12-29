using DSMOOFramework.Config;

namespace DSMOOWebInterface;

[Config(Name = "WebServer")]
public class Config : IConfig
{
    public string Url { get; set; } = "http://+";
    public ushort Port { get; set; } = 7832;
    public string AdminPassword { get; set; } = "";
}