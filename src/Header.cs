public struct Header
{
    public int Unknown1;
    public int Version; // Sempre -2 (DTX_VERSION)
    public ushort Width;
    public ushort Height;
    public ushort MipmapCount; // Pode ser mipmaps ou bytes por pixel, frequentemente 4
    public ushort HasLights;
    public Flags Flags; // Flags em formato hexadecimal
    public uint SurfaceFlags; // Tipo de superfície (ex.: pedra, metal, etc.)
    public byte Group;
    public byte MipmapsUsedCount; // 0 = 4?
    public byte AlphaCutoff; // Limitado a [128-255]
    public byte AlphaAverage;
    public uint Unknown2;
    public uint Unknown3;
    public byte Unknown4;
    public byte Unknown5;
    public ushort Unknown6;
    public byte Unknown7;
    public byte Unknown8;
    public ushort Unknown9;

    public static Header Read(BinaryReader reader)
    {
        var ret = new Header();

        ret.Unknown1 = reader.ReadInt32();
        ret.Version = reader.ReadInt32(); // DTX_VERSION = -2
        ret.Width = reader.ReadUInt16();
        ret.Height = reader.ReadUInt16();
        ret.MipmapCount = reader.ReadUInt16();
        ret.HasLights = reader.ReadUInt16();
        
        var flags = reader.ReadUInt32();
        ret.Flags = (Flags)Convert.ToInt32(flags);
        
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