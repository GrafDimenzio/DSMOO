using DSMOOPlus.Commands;
using DSMOOPlus.Enum;
using DSMOOPlus.Packets;
using DSMOOServer.API.Player;

namespace DSMOOPlus;

public class PlayerPlus : PlayerComponent
{
    public PlayerCommandSender CommandSender { get; internal set; }
    
    public async Task SendMessage(string message, MessageType messageType = MessageType.System, Guid? sender = null)
    {
        await Player.Send(new MessagePacket
        {
            MessageType = messageType,
            Message = message,
            SenderId = (uint)sender.GetHashCode()
        }, sender);
    }

    public async Task SetHealth(byte health, bool useAssistHealth = false)
    {
        await Player.Send(new PlayerStatePacket()
        {
            Health = health,
            AssistModeHealth = useAssistHealth
        }, null);
    }
}