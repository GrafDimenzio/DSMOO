using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DSMOOServer.API.Player;
using DSMOOServer.Logic;
using EmbedIO.WebSockets;

namespace DSMOOProximityVoiceChat;

public class VoiceChatWebSocket : WebSocketModule
{
    private readonly PlayerManager _playerManager;
    
    public VoiceChatWebSocket(string urlPath, bool enableConnectionWatchdog, PlayerManager playerManager) : base(urlPath, enableConnectionWatchdog)
    {
        _playerManager = playerManager;
        Task.Run(VolumeLoop);
    }
    
    private const float MaxDistance = 3500f;
    private readonly Dictionary<string, IWebSocketContext> _clients = new();
    
    protected override Task OnClientConnectedAsync(IWebSocketContext context)
    {
        var username = context.Session["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            return context.WebSocket.CloseAsync();
        }
        
        lock (_clients)
        {
            _clients[username] = context;
        }

        foreach (var webSocketContext in ActiveContexts)
        {
            if(webSocketContext == context) continue;
            SendAsync(webSocketContext, JsonSerializer.Serialize(new
            {
                type = "user-joined",
                id = username
            }));
        }
        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context)
    {
        var username = context.Session["username"] as string;
        if (string.IsNullOrEmpty(username))
        {
            return context.WebSocket.CloseAsync();
        }
        
        lock (_clients)
        {
            _clients.Remove(username);
        }
        BroadcastAsync(JsonSerializer.Serialize(new
        {
            type = "user-left",
            id = username
        }));
        return Task.CompletedTask;
    }

    protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
    {
        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var msg = JsonNode.Parse(json);
        if (msg == null) return Task.CompletedTask;
        var username = context.Session["username"] as string;
        var type = msg["type"]?.GetValue<string>();

        switch (type)
        {
            case "offer":
            case "answer":
            case "ice":
                ForwardSignaling(username!, msg);
                break;
        }
        
        return Task.CompletedTask;
    }

    private void ForwardSignaling(string from, JsonNode msg)
    {
        var to = msg["to"]?.GetValue<string>();
        if (to == null) return;
        msg["from"] = from;
        lock (_clients)
        {
            if (_clients.TryGetValue(to, out var target))
            {
                target.WebSocket.SendAsync(Encoding.UTF8.GetBytes(msg.ToJsonString()), true);
            }
        }
    }

    private async Task VolumeLoop()
    {
        while (true)
        {
            await Task.Delay(50);
            
            if(_clients.Count == 0)
                continue;

            var connectedPlayers = _playerManager.RealPlayers.Where(x => _clients.ContainsKey(x.Name)).ToList();

            foreach (var listener in connectedPlayers)
            {
                foreach (var speaker in connectedPlayers)
                {
                    if (listener.Name == speaker.Name) continue;
                    if (!_clients.TryGetValue(listener.Name, out var webSocket))
                        continue;
                    var volume = ComputeVolume(listener, speaker);
                    var msg = JsonSerializer.Serialize(new
                    {
                        type = "volume",
                        from = speaker.Name,
                        value = volume
                    });
                    await webSocket.WebSocket.SendAsync(Encoding.UTF8.GetBytes(msg), true);
                }
            }
        }
    }
    
    private float ComputeVolume(IPlayer listener, IPlayer speaker)
    {
        if (listener.Stage != speaker.Stage)
        {
            return 0;
        }
        var dist = (listener.Position - speaker.Position).Length();
        return Math.Clamp(1f - (dist / MaxDistance), 0f, 1f);
    }
}