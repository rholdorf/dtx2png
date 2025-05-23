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
    public ushort Fmt1;
    public int Fmt2;
    public uint SurfaceFlags;
    public byte Fmt3;
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
        var ret = new Header
        {
            Supported = true
        };

        if (reader.BaseStream.Length < 8)
        {
            Debug.WriteLine("File too small to be a DTX");
            ret.Supported = false;
            return ret;
        };
        
        ret.Unknown1 = reader.ReadInt32();
        ret.Version = reader.ReadInt32();

        if (ret.Unknown1 != 0 || ret.Version != -5)
        {
            Debug.WriteLine("Unknown or Version not matching");
            ret.Supported = false;
            return ret;
        }

        ret.Width = reader.ReadUInt16();
        ret.Height = reader.ReadUInt16();
        
        ret.MipmapCount = reader.ReadUInt16(); // uint
        ret.Fmt1 = reader.ReadUInt16(); // uint
        ret.Fmt2 = (int)reader.ReadUInt32(); // uint
        ret.SurfaceFlags = reader.ReadUInt32(); // short
        ret.Fmt3 = reader.ReadByte(); // ushort

        reader.BaseStream.Seek(135, SeekOrigin.Current); // go to pixel data
        return ret;
    }

}
