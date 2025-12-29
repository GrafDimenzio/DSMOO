using System.Numerics;

namespace DSMOOServer.API.Serialized;

public class SerializedVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public static implicit operator Vector3(SerializedVector3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static implicit operator SerializedVector3(Vector3 v)
    {
        return new SerializedVector3 { X = v.X, Y = v.Y, Z = v.Z };
    }
}