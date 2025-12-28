using System.Buffers;
using System.Diagnostics;
using DSMOOServer.API.Player;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Recording;

public class Recording
{
    private readonly PacketManager _packetManager;
    
    private readonly Stopwatch _stopwatch = new();

    public Recording(PacketManager packetManager)
    {
        _packetManager = packetManager;
    }

    public Recording(string path, PacketManager packetManager)
    {
        _packetManager = packetManager;
        var fileInfo = new FileInfo(path);
        var length = (int)fileInfo.Length;
        var memory = MemoryPool<byte>.Shared.RentZero(length);
        using (var fs = File.OpenRead(path))
        {
            fs.ReadExactly(memory.Memory.Span[..length]);
        }

        var current = 0;
        while (current < length)
        {
            var header = new RecordingHeader();
            header.Deserialize(memory.Memory.Span[current..(current + header.Size)]);
            current += header.Size;
            var packet = (IPacket)Activator.CreateInstance(packetManager.GetPacketType(header.Type))!;
            packet.Deserialize(memory.Memory.Span[current..(current + packet.Size)]);

            current += packet.Size;
            Elements.Add(new RecordingElement
            {
                Header = header,
                Packet = packet
            });
        }
    }

    public bool IsRecording => _stopwatch.IsRunning;

    public List<RecordingElement> Elements { get; set; } = [];

    public void Reset()
    {
        _stopwatch.Reset();
        Elements.Clear();
    }

    public void StartRecording(IPlayer player)
    {
        StartRecording([
            new CostumePacket
            {
                BodyName = player.Costume.BodyName,
                CapName = player.Costume.CapName
            },
            new GamePacket
            {
                Is2d = player.Is2d,
                Stage = player.Stage,
                ScenarioNum = player.Scenario
            },
            new PlayerPacket
            {
                Act = player.Act,
                Position = player.Position,
                Rotation = player.Rotation,
                SubAct = player.SubAct,
                AnimationBlendWeights = player.AnimationBlendWeights
            },
            new TagPacket
            {
                Minutes = player.Time.Minutes,
                Seconds = player.Time.Seconds,
                GameMode = player.CurrentGameMode,
                IsIt = player.IsIt,
                UpdateType = TagPacket.TagUpdate.Both
            }
        ]);
    }

    public void StartRecording(IPacket[] startingPackets)
    {
        _stopwatch.Reset();
        _stopwatch.Start();

        var delay = 0;

        foreach (var startingPacket in startingPackets)
        {
            AddPacket(startingPacket, delay);
            delay += 10;
        }
    }

    public void StopRecording()
    {
        _stopwatch.Reset();
    }

    public void AddPacket(IPacket packet)
    {
        if (!_stopwatch.IsRunning || packet.Equals(Elements.Last().Packet))
            return;
        AddPacket(packet, (int)_stopwatch.ElapsedMilliseconds);
        _stopwatch.Restart();
    }

    public void SaveToFile(string fileName)
    {
        var size = 0;
        foreach (var element in Elements)
            size += element.Size;
        var memory = MemoryPool<byte>.Shared.RentZero(size);
        var current = 0;
        foreach (var element in Elements)
        {
            element.Header.Serialize(memory.Memory.Span[current..(current + element.Header.Size)]);
            current += element.Header.Size;
            element.Packet.Serialize(memory.Memory.Span[current..(current + element.Packet.Size)]);
            current += element.Packet.Size;
        }

        File.WriteAllBytes(fileName, memory.Memory.Span[..size].ToArray());
        memory.Dispose();
    }

    private void AddPacket(IPacket packet, int timestamp)
    {
        Elements.Add(new RecordingElement
        {
            Header = new RecordingHeader
            {
                Timestamp = timestamp,
                Type = _packetManager.GetPacketId(packet.GetType())
            },
            Packet = packet
        });
    }

    public class RecordingElement
    {
        public required RecordingHeader Header;
        public required IPacket Packet;
        public int Size => Header.Size + Packet.Size;
    }
}