using DSMOOFramework.Managers;
using DSMOOFramework.Plugins;
using DSMOOWebInterface;
using DSMOOWebInterface.Models;
using DSMOOWebInterface.Setup.Template;

namespace DSMOOProxmityVoiceChat;

[Plugin(
    Author = "Dimenzio",
    Name = "DSMOOProxmityVoiceChat",
    Description = "Proximty Voice Chat via WebInterface",
    Repository = "https://github.com/GrafDimenzio/DSMOO",
    Version = "1.0.0"
)]
public class WebServerManager(TemplateManager templateManager, WebServer webServer) : Manager
{
    public override void Initialize()
    {
        templateManager.LoadTemplates(GetType().Assembly, "DSMOOProxmityVoiceChat.Templates");
        templateManager.AddNavigation(new NavigationElementModel
        {
            Href = "/proximity/chat",
            Svg =
                "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"24\" height=\"24\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"lucide lucide-mic-icon lucide-mic\"><path d=\"M12 19v3\"/><path d=\"M19 10v2a7 7 0 0 1-14 0v-2\"/><rect x=\"9\" y=\"2\" width=\"6\" height=\"13\" rx=\"3\"/></svg>",
            Text = "Proximity Voice Chat",
            SelectionId = 6547,
            PlayerLoginRequired = true
        });
    }
}