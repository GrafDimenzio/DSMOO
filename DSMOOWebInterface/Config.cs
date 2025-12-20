using DSMOOFramework.Config;

namespace DSMOOWebInterface;

[Config(Name = "WebServer")]
public class Config : IConfig
{
    public string Url { get; set; } = "127.0.0.1";
    public ushort Port { get; set; }
}