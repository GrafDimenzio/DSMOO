using DSMOOServer.Logic;
using DSMOOWebInterface.Models;
using DSMOOWebInterface.Setup;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;

namespace DSMOOWebInterface.Controllers;

[Controller(Route = "/", ControllerType = ControllerType.Html)]
public class FrontEndController(TemplateManager manager, PlayerManager playerManager) : TemplateController(manager)
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
        var players = playerManager.RealPlayers.Select(x => new PlayerModel(x)).ToList();
        
        if(players.Count == 0)
            return await RenderTemplate("playerlistNone.html");
        
        return await RenderTemplate("playerlist.html",
            new { Players = players });
    }
    
    [Route(HttpVerbs.Get, "/player")]
    public async Task<string> NoPlayer()
    {
        return await RenderTemplate("noPlayer.html");
    }
    
    [Route(HttpVerbs.Get, "/player/{id}")]
    public async Task<string> Player(string id)
    {
        if (Guid.TryParse(id, out var guid) && playerManager.RealPlayers.Any(x => x.Id == guid))
            return await RenderTemplate("player.html",
                new PlayerModel(playerManager.RealPlayers.First(x => x.Id == guid)));
        
        return await RenderTemplate("noPlayer.html");
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