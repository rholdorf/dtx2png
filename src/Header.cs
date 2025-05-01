using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace dtx2png;

public struct Header
{
    public int Unknown1;
    public int Version;
    public ushort Width;
    public ushort Height;
    public ushort MipmapCount;
    public ushort HasLights;
    public int Flags;
    public uint SurfaceFlags;
    public byte Group;
    public byte MipmapsUsedCount;
    public byte AlphaCutoff;
    public byte AlphaAverage;
    public uint Unknown2;
    public uint Unknown3;
    public byte Unknown4;
    public byte Unknown5;
    public ushort Unknown6;
    public byte Unknown7;
    public byte Unknown8;
    public ushort Unknown9;
    public bool Supported;

    public static Header Read(BinaryReader reader)
    {
        var ret = new Header();
        
        if (reader.BaseStream.Length < 8)
        {
            Debug.WriteLine("File too small to be a DTX");
            ret.Supported = false;
            return ret;
        };
        
        ret.Unknown1 = reader.ReadInt32();
        ret.Version = reader.ReadInt32();
        
        if(ret.Unknown1!= 0 || ret.Version != -5)
        {
            Debug.WriteLine("Unknown1 or Version not matching");
            ret.Supported = false;
            return ret;
        }

        ret.Supported = true;
        ret.Width = reader.ReadUInt16();
        ret.Height = reader.ReadUInt16();
        ret.MipmapCount = reader.ReadUInt16();
        ret.HasLights = reader.ReadUInt16();

        ret.Flags = (int)reader.ReadUInt32();

        ret.SurfaceFlags = reader.ReadUInt32();
        ret.Group = reader.ReadByte();
        ret.MipmapsUsedCount = reader.ReadByte();
        ret.AlphaCutoff = reader.ReadByte();
        ret.AlphaAverage = reader.ReadByte();
        ret.Unknown2 = reader.ReadUInt32();
        ret.Unknown3 = reader.ReadUInt32();
        ret.Unknown4 = reader.ReadByte();
        ret.Unknown5 = reader.ReadByte();
        ret.Unknown6 = reader.ReadUInt16();
        ret.Unknown7 = reader.ReadByte();
        ret.Unknown8 = reader.ReadByte();
        ret.Unknown9 = reader.ReadUInt16();

        return ret;
    }
}