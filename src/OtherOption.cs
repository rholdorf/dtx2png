using System.Diagnostics;

namespace dtx2png;

public class LithTechTextureLoader
{
    public static bool CheckType(byte[] data)
    {
        if (data.Length < 8)
            return false;

        try
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            var int1 = reader.ReadUInt32();
            var int2 = reader.ReadInt32();

            return int1 == 0 && int2 == -5;
        }
        catch
        {
            return true;
        }
    }

    public static List<Texture> LoadRgba(byte[] data)
    {
        var parser = new SanaeParser(data);
        parser.ParseData();
        return parser.Textures;
    }
}

public class SanaeParser
{
    private readonly BinaryReader _reader;
    public List<Texture> Textures { get; }

    public SanaeParser(byte[] data)
    {
        this._reader = new BinaryReader(new MemoryStream(data));
        this.Textures = new List<Texture>();
    }

    public void ParseData()
    {
        var check = _reader.ReadUInt32();
        ushort width, height;

        if (check != 0)
        {
            _reader.BaseStream.Seek(-4, SeekOrigin.Current);
            height = _reader.ReadUInt16();
            width = _reader.ReadUInt16();
            _reader.ReadUInt32(); // skip
            _reader.ReadUInt32(); // skip
        }
        else
        {
            _reader.ReadInt32();
            height = _reader.ReadUInt16();
            width = _reader.ReadUInt16();
        }

        var mipmaps = _reader.ReadUInt32();
        var fmt1 = _reader.ReadUInt32();
        var fmt2 = _reader.ReadUInt32();
        var _ = _reader.ReadUInt16();
        var fmt3 = _reader.ReadUInt16();
        var unk2 = fmt3;

        _reader.BaseStream.Seek(136, SeekOrigin.Current); // go to pixel data

        var tex = GetPixels(width, height, fmt1, fmt2, fmt3);
        if (tex != null)
        {
            Textures.Add(tex);
        }
    }

    private Texture? GetPixels(ushort width, ushort height, uint fmt1, uint fmt2, ushort fmt3)
    {
        var dx = (width + 3) >> 2;
        var dy = (height + 3) >> 2;

        if (fmt3 >= 0 && fmt3 <= 3)
        {
            var pixelData = _reader.ReadBytes(4 * width * height);
            if (fmt1 == 0x8)
            {
                return new Texture("texture", width, height, pixelData, TextureFormat.RGB24);
            }

            return new Texture("texture", width, height, pixelData, TextureFormat.RGBA32);
        }
        
        if (fmt3 == 4)
        {
            var pixelData = _reader.ReadBytes(8 * dx * dy);
            return new Texture("texture", width, height, pixelData, TextureFormat.DXT1);
        }
        
        if (fmt3 == 6)
        {
            var pixelData = _reader.ReadBytes(16 * dx * dy);
            return new Texture("texture", width, height, pixelData, TextureFormat.DXT5);
        }
        
        Debug.WriteLine("Unable to determine texture format");
        return null;
    }
}

public class Texture
{
    public string Name { get; }
    public ushort Width { get; }
    public ushort Height { get; }
    public byte[] PixelData { get; }
    public TextureFormat Format { get; }

    public Texture(string name, ushort width, ushort height, byte[] pixelData, TextureFormat format)
    {
        Name = name;
        Width = width;
        Height = height;
        PixelData = pixelData;
        Format = format;
    }
}

public enum TextureFormat
{
    RGB24,
    RGBA32,
    DXT1,
    DXT5
}