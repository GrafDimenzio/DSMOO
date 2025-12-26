using Swan.Formatters;

namespace DSMOOWebInterface.Models;

public class ConsoleRequestModel
{
    [JsonProperty("command")]
    public string Command { get; set; } = "";
}