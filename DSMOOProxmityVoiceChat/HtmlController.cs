using DSMOOWebInterface.Setup;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;

namespace DSMOOProxmityVoiceChat;

[Controller(Route = "/proximity", ControllerType = ControllerType.Html)]
public class HtmlController(TemplateManager manager) : TemplateController(manager)
{
    [Route(HttpVerbs.Get, "/chat")]
    public async Task<string> Chat()
    {
        return await RenderTemplate("proximity_voice_chat.html");
    }
}