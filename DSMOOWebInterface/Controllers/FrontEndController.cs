using DSMOOWebInterface.Setup;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;

namespace DSMOOWebInterface.Controllers;

[Controller(Route = "/", ControllerType = ControllerType.Html)]
public class FrontEndController(TemplateManager manager) : TemplateController(manager)
{
    [Route(HttpVerbs.Get, "/")]
    public async Task<string> Status()
    {
        return await RenderTemplate("landing.html");
    }
    
    [Route(HttpVerbs.Get, "/dashboard")]
    public async Task<string> Dashboard()
    {
        return await RenderTemplate("dashboard.html");
    }
    
    [Route(HttpVerbs.Get, "/playerlist")]
    public async Task<string> PlayerList()
    {
        return await RenderTemplate("playerlist.html");
    }
    
    [Route(HttpVerbs.Get, "/console")]
    public async Task<string> Console()
    {
        return await RenderTemplate("console.html");
    }
    
    [Route(HttpVerbs.Get, "/map")]
    public async Task<string> Map()
    {
        return await RenderTemplate("map.html");
    }
    
    [Route(HttpVerbs.Get, "/login")]
    public void Login()
    {
        Session["username"] = "dimenzio";
        HttpContext.Redirect("/");
    }
}