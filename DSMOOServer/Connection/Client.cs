using System.Buffers;
using System.Net.Sockets;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOServer.API.Events;
using DSMOOServer.API.Player;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Connection;

public class Client : IDisposable
{
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly PacketManager _packetManager;

    public Client(Socket socket, ILogger logger, PacketManager packetManager, ObjectController objectController,
        EventManager eventManager)
    {
        _packetManager = packetManager;
        Logger = logger;
        Socket = socket;
    }

    public Player Player { get; internal set; }

    public ILogger Logger { get; }

    public Socket Socket { get; }

    public bool FirstPacketSend { get; internal set; } = false;

    public Guid Id { get; internal set; }

    public bool IsBanned { get; internal set; } = false;

    public bool GotMigrated { get; internal set; } = false;

    public string Name
    {
        get => Logger.Name;
        set => Logger.Name = value;
    }

    public bool Ignored { get; internal set; }

    public void Dispose()
    {
        try
        {
            if (Socket.Connected)
                Socket.Disconnect(false);
        }
        catch (ObjectDisposedException e)
        {
        }
        catch (SocketException e)
        {
        }

        Socket.Close();
        _sendLock.Dispose();
    }
    
    private async Task SendAll(ReadOnlyMemory<byte> data)
    {
        var totalSent = 0;

        while (totalSent < data.Length)
        {
            var sent = await Socket.SendAsync(
                data[totalSent..],
                SocketFlags.None);

            if (sent <= 0)
                throw new SocketException();

            totalSent += sent;
        }
    }


    public async Task Send(IPacket packet, Guid? sender = null)
    {
        if (!Socket.Connected)
            return;

        if (Ignored && packet.GetType() != typeof(ChangeStagePacket))
            return;

        if (!FirstPacketSend && packet.GetType() != typeof(ConnectPacket))
        {
            Logger.Error($"Didn't send {packet.GetType()} to {Id} because they weren't connected yet");
            return;
        }

        var memory = MemoryPool<byte>.Shared.RentZero(Constants.HeaderSize + packet.Size);
        
        var packetTypeId = _packetManager.GetPacketId(packet.GetType());
        try
        {
            PacketHelper.FillPacket(new PacketHeader
            {
                Id = sender ?? Id,
                Type = packetTypeId,
                PacketSize = packet.Size
            }, packet, memory.Memory);

            var packetMemory = memory.Memory[..(Constants.HeaderSize + packet.Size)];
            
            await _sendLock.WaitAsync();

            try
            {
                await SendAll(packetMemory);
            }
            finally
            {
                _sendLock.Release();
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to serialize {packetTypeId}", e);
        }

        memory.Dispose();
    }

    public async Task Send(Memory<byte> data)
    {
        if (!Socket.Connected)
            return;

        var header = new PacketHeader();
        header.Deserialize(data.Span);

        if (Ignored && header.Type != (short)PacketType.ChangeStage)
            return;

        if (!FirstPacketSend && header.Type != (short)PacketType.Connect)
        {
            Logger.Error($"Didn't send {header.Type} to {Id} because they weren't connected yet");
            return;
        }

        try
        {
            var packetMemory = data[..(Constants.HeaderSize + header.PacketSize)];

            await _sendLock.WaitAsync();

            try
            {
                await SendAll(packetMemory);
            }
            finally
            {
                _sendLock.Release();
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to send raw packet", e);
        }
    }

    public async Task Crash(bool ban)
    {
        await Send(new ChangeStagePacket
        {
            Stage = ban ? "$ejected" : "$agogusStage"
        });
        Ignored = true;
    }
}