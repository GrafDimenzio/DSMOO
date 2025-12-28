using DSMOOServer.Logic;
using DSMOOServer.Network.Packets;
using DSMOOWebInterface.Models;
using DSMOOWebInterface.Setup;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;

namespace DSMOOWebInterface.Controllers;

[Controller(Route = "/", ControllerType = ControllerType.Html)]
public class FrontEndController(TemplateManager manager, PlayerManager playerManager) : TemplateController(manager)
{
    [Route(HttpVerbs.Get, "/hide-and-seek")]
    public async Task<string> HideAndSeek()
    {
        if (!CheckPlayer())
            return "";
        return await RenderTemplate("hide_and_seek.html");
    }
    
    [Route(HttpVerbs.Get, "/")]
    public async Task<string> Status()
    {
        return await RenderTemplate("landing.html");
    }
    
    [Route(HttpVerbs.Get, "/dashboard")]
    public async Task<string> Dashboard()
    {
        if (!CheckAdmin())
            return "";
        var players = playerManager.RealPlayers;
        var playersInGameMode = players.Where(x =>
            x.CurrentGameMode is GameMode.Legacy or not GameMode.None).ToArray();
        var modelPlayers = players.Select(x => new PlayerModel(x)).ToList();
        return await RenderTemplate("dashboard.html",
            new
            {
                PlayerCount = playerManager.PlayerCount,
                It = playersInGameMode.Count(x => x.IsIt),
                NotIt = playersInGameMode.Count(x => !x.IsIt),
                players = modelPlayers
            });
    }
    
    [Route(HttpVerbs.Get, "/playerlist")]
    public async Task<string> PlayerList()
    {
        if (!CheckAdmin())
            return "";
        
        var players = playerManager.RealPlayers.Select(x => new PlayerModel(x)).ToList();
        
        if(players.Count == 0)
            return await RenderTemplate("playerlistNone.html");
        
        return await RenderTemplate("playerlist.html",
            new { Players = players });
    }
    
    [Route(HttpVerbs.Get, "/player")]
    public async Task<string> NoPlayer()
    {
        if (!CheckAdmin())
            return "";
        return await RenderTemplate("noPlayer.html");
    }
    
    [Route(HttpVerbs.Get, "/player/{id}")]
    public async Task<string> Player(string id)
    {
        if (!CheckAdmin())
            return "";
        if (Guid.TryParse(id, out var guid) && playerManager.RealPlayers.Any(x => x.Id == guid))
            return await RenderTemplate("player.html",
                new PlayerModel(playerManager.RealPlayers.First(x => x.Id == guid)));
        
        return await RenderTemplate("noPlayer.html");
    }
    
    [Route(HttpVerbs.Get, "/console")]
    public async Task<string> Console()
    {
        if (!CheckAdmin())
            return "";
        return await RenderTemplate("console.html");
    }
    
    [Route(HttpVerbs.Get, "/map")]
    public async Task<string> Map()
    {
        if (!CheckAdmin())
            return "";
        return await RenderTemplate("map.html");
    }
    
    [Route(HttpVerbs.Get, "/login")]
    public async Task<string> Login()
    {
        return await RenderTemplate("login.html");
    }

    public bool CheckAdmin()
    {
        if(Session.ContainsKey("admin") && Session["admin"] is true)
            return true;
        
        HttpContext.Redirect("/");
        return false;
    }
    
    public bool CheckPlayer()
    {
        if(Session.ContainsKey("username") && !string.IsNullOrWhiteSpace(Session["username"] as string))
            return true;
        
        HttpContext.Redirect("/");
        return false;
    }
}