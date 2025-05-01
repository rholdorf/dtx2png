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

    /// <summary>
    /// This DTX has full bright colors (this means palette index 255 will be unaffected by lighting)
    /// </summary>
    public bool FullBright;

    /// <summary>
    /// This texture contains alpha masks
    /// </summary>
    public bool AlphaMasks;

    /// <summary>
    /// Unknown
    /// </summary>
    public bool FlagUnknown1;

    /// <summary>
    /// Unknown, probably compatibility bit; seems to always be set
    /// </summary>
    public bool FlagUnknown2;

    /// <summary>
    /// Unknown
    /// </summary>
    public bool FlagUnknown3;

    /// <summary>
    /// Map to master palette
    /// </summary>
    public bool MapToMasterPalette;

    public static Header Read(BinaryReader reader)
    {
        var ret = new Header();

        ret.Unknown1 = reader.ReadInt32();
        ret.Version = reader.ReadInt32();
        ret.Width = reader.ReadUInt16();
        ret.Height = reader.ReadUInt16();
        ret.MipmapCount = reader.ReadUInt16();
        ret.HasLights = reader.ReadUInt16();

        var flags = reader.ReadUInt32();

        ret.FullBright = (flags & 0x01) != 0;
        ret.AlphaMasks = (flags & 0x02) != 0;
        ret.FlagUnknown1 = (flags & 0x04) != 0;
        ret.FlagUnknown2 = (flags & 0x08) != 0;
        ret.FlagUnknown3 = (flags & 0x10) != 0;
        ret.MapToMasterPalette = (flags & 0x20) != 0;

        ret.Flags = Convert.ToInt32(flags);

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