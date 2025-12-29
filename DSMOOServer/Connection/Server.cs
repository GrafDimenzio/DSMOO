using System.Buffers;
using System.Net;
using System.Net.Sockets;
using DSMOOFramework.Analyzer;
using DSMOOFramework.Config;
using DSMOOFramework.Controller;
using DSMOOFramework.Logger;
using DSMOOFramework.Managers;
using DSMOOServer.API.Events;
using DSMOOServer.API.Events.Args;
using DSMOOServer.Logic;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.Connection;

[Analyze(Priority = -1)] // That way the actual server will start last when everything else is initialised
public class Server(
    ILogger log,
    ConfigHolder<ServerMainConfig> configHolder,
    EventManager eventManager,
    PlayerManager playerManager,
    ObjectController objectController,
    PacketManager packetManager) : Manager
{
    private readonly MemoryPool<byte> _memoryPool = MemoryPool<byte>.Shared;

    public readonly List<Client> Clients = [];
    private bool _active;
    private ILogger Logger { get; } = log;
    private ServerMainConfig Config { get; } = configHolder.Config;
    private EventManager EventManager { get; } = eventManager;

    public override void Initialize()
    {
        Task.Run(Listen);
    }

    private async Task Listen()
    {
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(Config.Address), Config.Port));
            serverSocket.Listen();
        }
        catch (Exception ex)
        {
            Logger.Error("Error while setting up the Server", ex);
            return;
        }

        Logger.Info($"Listening on {Config.Address}:{Config.Port}");

        _active = true;
        while (_active)
            try
            {
                var socket = await serverSocket.AcceptAsync();
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                Logger.Info($"Accepted connection from {socket.RemoteEndPoint}");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await HandleSocket(socket);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error while handling socket", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Error while setting up Socket Handler", ex);
            }

        try
        {
            serverSocket.Shutdown(SocketShutdown.Both);
        }
        catch (Exception ex)
        {
            Logger.Error("Error while closing the Server", ex);
        }
        finally
        {
            serverSocket.Close();
            Logger.Info("Server closed");
        }
    }

    private async Task HandleSocket(Socket socket)
    {
        var client = new Client(socket, Logger.Copy(), packetManager, objectController, EventManager);
        playerManager.Players.Add(client.Player);
        IMemoryOwner<byte> memory = null!;
        var id = Guid.Empty;
        var endPointString = socket.RemoteEndPoint.ToString();

        try
        {
            while (socket.Connected)
            {
                memory = _memoryPool.Rent(Constants.HeaderSize);
                var (result, packetHeader) = await ReadPacketHeader(socket, memory);
                if (!result)
                    break;

                (result, memory) = await ReadPacketToMemory(socket, memory, packetHeader);
                if (!result)
                    break;

                if (client.Id != Guid.Empty && packetHeader.Id != client.Id)
                {
                    Logger.Warn($"Client {client.Socket.RemoteEndPoint} send Packet with ID of another client");
                    continue;
                }

                id = packetHeader.Id;

                //Ignored players will still be connected but the server won't accept any Packets from them
                if (client.Ignored)
                {
                    playerManager.Players.Remove(client.Player);
                    //If the player is getting kicked/banned while he can't receive a ChangeStagePacket he won't see the notification
                    //therefore this will send it a second time if he is still able to switch stages
                    if (packetHeader.Type == (short)PacketType.Game)
                        client.Player.Crash(client.Player.IsBanned);
                    continue;
                }

                try
                {
                    var packetType = packetManager.GetPacketType(packetHeader.Type);
                    if (packetType == null)
                        //The Client send an unknown Packet Type
                        //Most likely a modification that the Server doesn't Support
                        //Ignore the Packet and don't broadcast it to other Player
                        continue;
                    var packet = (IPacket?)Activator.CreateInstance(packetType);
                    if (packet == null) continue;

                    packet.Deserialize(memory.Memory.Span[Constants.HeaderSize..(Constants.HeaderSize + packet.Size)]);
                    Logger.Debug($"Received Packet {packet.GetType()} {client.Socket.RemoteEndPoint}");

                    var arg = new PacketReceivedEventArgs(packetHeader, packet, client);
                    EventManager.OnPacketReceived.RaiseEvent(arg);

                    if (arg.Broadcast)
                        await ReplaceBroadcast(arg.ReplacePacket ?? arg.Packet, client.Id, arg.SpecificReplacePackets);
                }
                catch (Exception ex)
                {
                    client.Logger.Error("Error while deserializing a packet", ex);
                }

                memory.Dispose();
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error while handling socket", ex);
            memory?.Dispose();
        }

        if (id != Guid.Empty)
            await Broadcast(new DisconnectPacket(), id);

        Logger.Info($"Client {client.Name} disconnected from EndPoint {endPointString}");

        Clients.Remove(client);
        playerManager.Players.Remove(client.Player);
        client.Dispose();
    }

    private async Task<bool> Read(Socket socket, Memory<byte> readMem, int readSize, int readOffset)
    {
        try
        {
            readSize += readOffset;
            while (readOffset < readSize)
            {
                var size = await socket.ReceiveAsync(readMem[readOffset..readSize], SocketFlags.None);
                if (size == 0)
                {
                    Logger.Info($"Socket {socket.RemoteEndPoint} disconnected");
                    if (socket.Connected) await socket.DisconnectAsync(false);
                    return false;
                }

                readOffset += size;
            }

            return true;
        }
        catch (SocketException e)
        {
            Logger.Info($"Socket {socket.RemoteEndPoint} disconnected");
            return false;
        }
    }

    private async Task<(bool, PacketHeader)> ReadPacketHeader(Socket socket, IMemoryOwner<byte> memory)
    {
        var header = new PacketHeader();

        if (!await Read(socket, memory.Memory[..Constants.HeaderSize], Constants.HeaderSize, 0))
            return (false, header);

        header.Deserialize(memory.Memory.Span[..Constants.HeaderSize]);
        return (true, header);
    }

    private async Task<(bool, IMemoryOwner<byte>)> ReadPacketToMemory(Socket socket, IMemoryOwner<byte> memory,
        PacketHeader header)
    {
        if (header.PacketSize == 0)
            return (true, memory);

        var temporaryMemory = memory;
        memory = _memoryPool.Rent(Constants.HeaderSize + header.PacketSize);
        temporaryMemory.Memory.Span[..Constants.HeaderSize].CopyTo(memory.Memory.Span[..Constants.HeaderSize]);
        temporaryMemory.Dispose();

        if (!await Read(socket, memory.Memory, header.PacketSize, Constants.HeaderSize))
            return (false, memory);

        return (true, memory);
    }

    public async Task ReplaceBroadcast(IPacket packet, Guid? sender, Dictionary<Guid, IPacket> replacePackets)
    {
        await Parallel.ForEachAsync(Clients, async (client, _) =>
        {
            if (client.Ignored || !client.FirstPacketSend)
                return;

            if (replacePackets.ContainsKey(client.Id))
            {
                await client.Send(replacePackets[client.Id], sender);
                return;
            }

            if (client.Id == sender) return;

            await client.Send(packet, sender);
        });
    }

    public async Task Broadcast(IPacket packet, Guid? sender)
    {
        var memory = MemoryPool<byte>.Shared.RentZero(Constants.HeaderSize + packet.Size);
        var header = new PacketHeader
        {
            Id = sender ?? Guid.Empty,
            Type = packetManager.GetPacketId(packet.GetType()),
            PacketSize = packet.Size
        };
        PacketHelper.FillPacket(header, packet, memory.Memory);
        await Broadcast(memory, sender);
        memory.Dispose();
    }

    public async Task Broadcast(IMemoryOwner<byte> data, Guid? sender = null)
    {
        await Parallel.ForEachAsync(Clients.Where(x => x is { Ignored: false, FirstPacketSend: true }),
            async (client, _) =>
            {
                if (client.Id == sender)
                    return;
                await client.Send(data.Memory);
            });
    }
}