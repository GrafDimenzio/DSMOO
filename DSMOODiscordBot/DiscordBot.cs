using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSMOOFramework.Commands;
using DSMOOFramework.Logger;
using DSMOOFramework.Plugins;
using Microsoft.Extensions.Logging;

namespace DSMOODiscordBot;

[Plugin(
    Name = "DiscordBot",
    Description = "This Plugins adds a simple Discord bot for logging and commands",
    Author = "Dimenzio",
    Version = "1.0.0",
    Repository = "https://github.com/GrafDimenzio/DSMOO"
)]
public class DiscordBot(CommandManager commandManager) : Plugin<Config>
{
    private DiscordClient? _client;
    private DiscordChannel? _commandChannel;
    private DiscordChannel? _logChannel;
    private string? _mentionPrefix;

    private bool _reconnecting;

    public override void Initialize()
    {
        if (string.IsNullOrWhiteSpace(Config.Token))
        {
            Logger.Warn("No Token set for DiscordBot!");
            return;
        }
        
        if(Logger is BasicLogger basicLogger)
            basicLogger.Log += OnLog;

        Task.Run(Run);
    }

    private void OnLog(string message, LogType type)
    {
        try
        {
            if (_client == null || _logChannel == null) return;
            foreach (var msg in SplitMessage(message))
                _client.SendMessageAsync(_logChannel, msg);
        }
        catch (Exception ex)
        {
            if (_reconnecting) return;
            Logger.Error("Error while logging to discord", ex);
        }
    }
    
    private static List<string> SplitMessage(string message, int maxSizePerElem = 2000)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < message.Length; i += maxSizePerElem) 
        {
            result.Add(message.Substring(i, message.Length - i < maxSizePerElem ? message.Length - i : maxSizePerElem));
        }
        return result;
    }

    public async Task Reconnect()
    {
        _reconnecting = true;
        if(_client != null)
            await _client.DisconnectAsync();
        await Run();
    }

    public async Task Run()
    {
        _client?.Dispose();
        if (string.IsNullOrWhiteSpace(Config.Token))
        {
            Logger.Warn("No Token set for DiscordBot!");
            return;
        }

        try
        {
            _client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.Token,
                MinimumLogLevel = LogLevel.None
            });
            await _client.ConnectAsync(new DiscordActivity("Super Mario Odyssey Online", ActivityType.Competing));
            if (Config.CommandChannel != 0)
            {
                try
                {
                    _commandChannel = await _client.GetChannelAsync(Config.CommandChannel);
                }
                catch (Exception ex)
                {
                    Logger.Error("Couldn't get Discord Command Channel", ex);
                }
            }
            else
            {
                _commandChannel = null;
            }
            
            if (Config.LogChannel != 0)
            {
                try
                {
                    _logChannel = await _client.GetChannelAsync(Config.LogChannel);
                }
                catch (Exception ex)
                {
                    Logger.Error("Couldn't get Discord Log Channel", ex);
                }
            }
            else
            {
                _logChannel = null; 
            }
            
            Logger.Info($"Discord bot logged in as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");
            _reconnecting = false;
            _mentionPrefix = $"{_client.CurrentUser.Mention}";
            _client.MessageCreated += ClientOnMessageCreated;
            _client.ClientErrored += ClientOnClientErrored;
            _client.SocketErrored += ClientOnSocketErrored;
        }
        catch (Exception ex)
        {
            Logger.Error("Exception occurred in discord runner!", ex);
        }
    }

    private Task ClientOnSocketErrored(DiscordClient sender, SocketErrorEventArgs e)
    {
        Logger.Error("Discord client caught an error on socket!", e.Exception);
        return Task.CompletedTask;
    }

    private Task ClientOnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
    {
        Logger.Error("Discord client caught an error in handler!", e.Exception);
        return Task.CompletedTask;
    }

    private async Task ClientOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if(e.Author.IsCurrent) return;
        if(e.Channel is DiscordDmChannel) return;
        if(e.Channel.Id != _commandChannel?.Id) return;
        try
        {
            var msg = e.Message?.Content ?? "";
            if (msg.StartsWith(_mentionPrefix))
            {
                msg = msg[(_mentionPrefix.Length)..];
            }
            else if (string.IsNullOrWhiteSpace(Config.Prefix))
            {
                
            }
            else if (msg.StartsWith(Config.Prefix))
            {
                msg = msg[(Config.Prefix.Length)..];
            }
            else
            {
                return;
            }

            var response = commandManager.ProcessQuery(msg);
            foreach (var splitMessage in SplitMessage(response.Message))
                await e.Message?.RespondAsync(splitMessage);

        }
        catch (Exception exception)
        {
            Logger.Error("Exception occurred in discord bot while parsing a command", exception);
        }
    }
}