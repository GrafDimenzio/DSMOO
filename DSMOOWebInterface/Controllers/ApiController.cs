using DSMOOFramework.Commands;
using DSMOOWebInterface.Models;
using DSMOOWebInterface.Setup.Controller;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace DSMOOWebInterface.Controllers;

[Controller(Route = "/api", ControllerType = ControllerType.Json)]
public class ApiController(CommandManager commandManager) : WebApiController
{
    [Route(HttpVerbs.Post, "/console")]
    public async Task<object> Console()
    {
        if (Session["admin"] is not true)
        {
            return HttpException.Forbidden("You need to be logged in to do this.");
        }
        var request = await HttpContext.GetRequestDataAsync<ConsoleRequestModel>();
        return commandManager.ProcessQuery(request?.Command ?? "");
    }
}