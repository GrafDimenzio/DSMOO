using DSMOOFramework.Commands;
using DSMOOWebInterface.Models;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace DSMOOWebInterface.Controllers;

[Controller(Route = "/api", ControllerType = ControllerType.Json)]
public class ApiController(CommandManager commandManager, LoginManager manager) : WebApiController
{
    [Route(HttpVerbs.Post, "/console")]
    public async Task<object> Console()
    {
        if (Session["admin"] is not true) return HttpException.Forbidden("You need to be logged in to do this.");
        var request = await HttpContext.GetRequestDataAsync<ConsoleRequestModel>();
        return commandManager.ProcessQuery(request?.Command ?? "");
    }

    [Route(HttpVerbs.Post, "/player-login")]
    public void PlayerLogin([FormField] string username)
    {
        Session["username"] = username;
        HttpContext.Redirect("/hide-and-seek");
    }

    [Route(HttpVerbs.Post, "/admin-login")]
    public void AdminLogin([FormField] string password)
    {
        Session["admin"] = true;
        HttpContext.Redirect("/dashboard");
    }

    [Route(HttpVerbs.Post, "/player-logout")]
    public void PlayerLogout()
    {
        Session.TryRemove("username", out _);
        HttpContext.Redirect("/");
    }

    [Route(HttpVerbs.Post, "/admin-logout")]
    public void AdminLogout()
    {
        Session.TryRemove("admin", out _);
        HttpContext.Redirect("/");
    }
}