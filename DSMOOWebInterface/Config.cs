using DSMOOFramework.Config;

namespace DSMOOWebInterface;

[Config(Name = "WebServer")]
public class Config : IConfig
{
    public string Url { get; set; } = "http://0.0.0.0";
    public ushort Port { get; set; } = 7832;
}