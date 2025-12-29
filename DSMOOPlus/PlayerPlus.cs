using DSMOOPlus.Enum;
using DSMOOPlus.Packets;
using DSMOOServer.API.Player;

namespace DSMOOPlus;

public class PlayerPlus : PlayerComponent
{
    public async Task SendMessage(string message, MessageType messageType = MessageType.System, Guid? sender = null)
    {
        await Player.Send(new MessagePacket()
        {
            MessageType = messageType,
            Message = message,
            SenderId = (uint)sender.GetHashCode(),
        }, sender);
    }
}