using DSMOOWebInterface.Setup;
using EmbedIO;
using EmbedIO.Routing;

namespace DSMOOWebInterface.Controllers;

public class FrontEndController(TemplateManager manager) : TemplateController(manager)
{
    [Route(HttpVerbs.Get, "/")]
    public async Task<string> Status()
    {
        return await RenderTemplate("Test.html");
    }
    
    [Route(HttpVerbs.Get, "/invalid")]
    public async Task<string> Invalid()
    {
        return await RenderTemplate("szrhjgbfhjkhfgjkn.html");
    }
}