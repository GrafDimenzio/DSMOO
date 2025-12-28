using System.Numerics;

namespace DSMOOServer.API.Serialized;

public class SerializedVector3
{
    public float X { get; set; } = 0;
    public float Y { get; set; } = 0;
    public float Z { get; set; } = 0;

    public static implicit operator Vector3(SerializedVector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator SerializedVector3(Vector3 v) => new() { X = v.X, Y = v.Y, Z = v.Z };
}