using DSMOOFramework.Config;

namespace DSMOODiscordBot;

[Config(Name = "discord_bot")]
public class Config : IConfig
{
    public string Token { get; set; } = "";
    public string Prefix { get; set; } = "";
    public ulong CommandChannel { get; set; }
    public ulong LogChannel { get; set; }
}