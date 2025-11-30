using System.Buffers;
using System.Diagnostics;
using System.Text;
using DSMOOServer.API.Player;
using DSMOOServer.Network;
using DSMOOServer.Network.Packets;

namespace DSMOOServer.API.Recording;

public class Recording
{
    public Recording() {}

    public Recording(string path)
    {
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
            var packet = (IPacket)Activator.CreateInstance(Constants.PacketIdMap[header.Type])!;
            packet.Deserialize(memory.Memory.Span[current..(current + packet.Size)]);
            
            current += packet.Size;
            Elements.Add(new RecordingElement()
            {
                Header = header,
                Packet = packet,
            });
        }
    }
    
    private readonly Stopwatch _stopwatch = new Stopwatch();
    
    public bool IsRecording => _stopwatch.IsRunning;

    public List<RecordingElement> Elements { get; set; } = [];

    public void StartRecording(IPlayer player)
    {
        StartRecording([
            new CostumePacket()
            {
                BodyName = player.Costume.BodyName,
                CapName = player.Costume.CapName,
            },
            new GamePacket()
            {
                Is2d = player.Is2d,
                Stage = player.Stage,
                ScenarioNum = player.Scenario
            },
            new PlayerPacket()
            {
                Act = player.Act,
                Position = player.Position,
                Rotation = player.Rotation,
                SubAct = player.SubAct,
                AnimationBlendWeights = player.AnimationBlendWeights,
            },
            new TagPacket()
            {
                Minutes = player.Time.Minutes,
                Seconds = player.Time.Seconds,
                GameMode = player.CurrentGameMode,
                IsIt = player.IsIt,
                UpdateType = TagPacket.TagUpdate.Both
            }
        ]);
    }

    public void StopRecording() => _stopwatch.Reset();

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

    public void AddPacket(IPacket packet)
    {
        if(!_stopwatch.IsRunning || packet.Equals(Elements.Last().Packet))
            return;
        AddPacket(packet, (int)_stopwatch.ElapsedMilliseconds);
        _stopwatch.Restart();
    }

    public void SaveToFile(string fileName)
    {
        var size = 0;
        foreach (var element in Elements)
        {
            size += element.Size;
        }
        var memory = MemoryPool<byte>.Shared.RentZero(size);
        var current = 0;
        foreach (var element in Elements)
        {
            element.Header.Serialize(memory.Memory.Span[current..(current + element.Header.Size)]);
            current += element.Header.Size;
            element.Packet.Serialize(memory.Memory.Span[current..(current + element.Packet.Size)]);
            current += element.Packet.Size;
        }
        File.WriteAllBytes(fileName, memory.Memory.Span[0..size].ToArray());
        memory.Dispose();
    }

    private void AddPacket(IPacket packet, int timestamp)
    {
        Elements.Add(new RecordingElement()
        {
            Header = new RecordingHeader()
            {
                Timestamp = timestamp,
                Type = Constants.PacketMap[packet.GetType()].Type,
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