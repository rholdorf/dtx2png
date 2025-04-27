using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dtx2png <input.dtx> <output.png>");
            return;
        }

        var inputPath = args[0];
        var outPath = args[1];

            var dtxFile = DtxFile.ReadFromFile(inputPath);
            Console.WriteLine($"DTX File Loaded:");
            Console.WriteLine($"Width: {dtxFile.Header.Width}, Height: {dtxFile.Header.Height}");
            Console.WriteLine($"Mipmap Count: {dtxFile.Header.MipmapCount}");
            Console.WriteLine($"Flags: 0x{dtxFile.Header.Flags:X}");
            Console.WriteLine($"Surface Flags: 0x{dtxFile.Header.SurfaceFlags:X}");
            Console.WriteLine($"Has Lights: {dtxFile.Header.HasLights != 0}");
            Console.WriteLine($"Colours Count: {dtxFile.Colours?.Length}");
            Console.WriteLine($"Mipmaps Count: {dtxFile.Mipmaps?.Length}");
            if (dtxFile.AlphaMaps != null)
            {
                Console.WriteLine($"Alpha Maps Count: {dtxFile.AlphaMaps.Length}");
            }
            if (dtxFile.Header.HasLights !=0 && dtxFile.Lights != null)
            {
                Console.WriteLine($"Lights Count: {dtxFile.Lights.String.Length}");
                Console.WriteLine($"Light Definitions: {dtxFile.Lights.LightDefs}");
            }
            else
            {
                Console.WriteLine("No lights data present.");
            }
            
            // converte para PNG
            if (dtxFile.Header.Flags == Flags.Unknown2)
            {
                using var img = new Image<Rgba32>(dtxFile.Header.Width, dtxFile.Header.Height);
                var i = 0;
                for (var y = 0; y < dtxFile.Header.Height; y++)
                {
                    for (var x = 0; x < dtxFile.Header.Width; x++)
                    {
                        var color = dtxFile.PixelData[i];
                        img[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
                        i++;
                    }
                }

                img.Save(outPath);                
            }
            else
            {
                for (var i = 0; i < dtxFile.Mipmaps.Length; i++)
                {
                    var currentMipmap = dtxFile.Mipmaps[i];
                    var outputPath = Path.ChangeExtension(inputPath, $"_mipmap_{i}.png");

                    using var img = new Image<Rgba32>(dtxFile.Header.Width, dtxFile.Header.Height);
                    for (var y = 0; y < dtxFile.Header.Height; y++)
                    {
                        for (var x = 0; x < dtxFile.Header.Width; x++)
                        {
                            var palleteIndex = currentMipmap.Data[x, y];
                            var color = dtxFile.Colours[palleteIndex];
                            img[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
                        }
                    }

                    img.Save(outputPath);
                    Console.WriteLine($"Saved mipmap {i} to {outputPath}");
                }
            }


    }
}

// Classe principal para leitura do arquivo DTX
public class DtxFile
{
    public Header Header;
    public Colour[] Colours;
    public Pixels[] Mipmaps;
    public AlphaMap[] AlphaMaps;
    public Lights? Lights;
    public Colour[] PixelData;

    public static DtxFile ReadFromFile(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var dtxFile = new DtxFile
        {
            Header = Header.Read(reader)
        };

        // Verifica se o arquivo é válido
        if (dtxFile.Header.Version != -2)
        {
            //throw new InvalidDataException("Invalid DTX file: Version is not -2.");
        }

        if (dtxFile.Header.Flags == Flags.Unknown2)
        {
            reader.BaseStream.Seek(120, SeekOrigin.Current);            
            dtxFile.PixelData = new Colour[dtxFile.Header.Width * dtxFile.Header.Height];
            for (var i = 0; i < dtxFile.PixelData.Length; i++)
            {
                dtxFile.PixelData[i] = Colour.Read(reader);
            }
        }
        else
        {
            // Leitura da paleta de cores (sempre 256 cores)
            dtxFile.Colours = new Colour[256];
            for (var i = 0; i < 256; i++)
            {
                dtxFile.Colours[i] = Colour.Read(reader);
            }

            // Leitura dos mipmaps
            dtxFile.Mipmaps = new Pixels[dtxFile.Header.MipmapCount];
            for (var i = 0; i < dtxFile.Header.MipmapCount; i++)
            {
                var mipWidth = dtxFile.Header.Width >> i;
                var mipHeight = dtxFile.Header.Height >> i;
                if (mipWidth < 1) mipWidth = 1;
                if (mipHeight < 1) mipHeight = 1;
                dtxFile.Mipmaps[i] = Pixels.Read(reader, mipWidth, mipHeight);
            }

            // Leitura dos mapas de alfa (se DTX_ALPHA_MASKS estiver definido)
            if (dtxFile.Header.Flags == Flags.AlphaMasks) // DTX_ALPHA_MASKS
            {
                dtxFile.AlphaMaps = new AlphaMap[dtxFile.Header.MipmapCount];
                for (var i = 0; i < dtxFile.Header.MipmapCount; i++)
                {
                    var mipWidth = dtxFile.Header.Width >> i;
                    var mipHeight = dtxFile.Header.Height >> i;
                    if (mipWidth < 1) mipWidth = 1;
                    if (mipHeight < 1) mipHeight = 1;
                    dtxFile.AlphaMaps[i] = AlphaMap.Read(reader, mipWidth, mipHeight);
                }
            }

            // Leitura dos dados de luz (se has_lights for verdadeiro)
            if (dtxFile.Header.HasLights != 0)
            {
                dtxFile.Lights = Lights.Read(reader);
            }
        }

        return dtxFile;
    }
}

// Estrutura para dados de luzes
public class Lights
{
    public string LightDefs; // Dados de luz (tamanho variável)
    public byte[] Unknown; // 18 bytes desconhecidos
    public LTLongString String;

    public static Lights Read(BinaryReader reader)
    {
        var lights = new Lights
        {
            // LIGHTDEFS é uma string de tamanho variável, não especificada no formato
            // Vamos assumir que é uma string terminada em null ou de tamanho fixo
            // Aqui, você pode precisar ajustar com base em engenharia reversa
            LightDefs = ReadNullTerminatedString(reader),
            Unknown = reader.ReadBytes(18),
            String = LTLongString.Read(reader)
        };
        return lights;
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var sb = new StringBuilder();
        byte b;
        while ((b = reader.ReadByte()) != 0)
        {
            sb.Append((char)b);
        }
        return sb.ToString();
    }
}

// Estrutura para strings longas com comprimento prefixado
public struct LTLongString
{
    public uint Length;
    public string Content;

    public static LTLongString Read(BinaryReader reader)
    {
        var ltLongString = new LTLongString
        {
            Length = reader.ReadUInt32(),
            Content = Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32()))
        };
        return ltLongString;
    }
}

// Estrutura para mapa de alfa (4 bits por pixel)
public struct AlphaMap
{
    public byte[,] Alpha; // Matriz de valores de alfa (4 bits por pixel)

    public static AlphaMap Read(BinaryReader reader, int width, int height)
    {
        var alphaMap = new AlphaMap
        {
            Alpha = new byte[height, width]
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width / 2; x++) // Cada byte contém dois valores de alfa (4 bits cada)
            {
                var alphaByte = reader.ReadByte();
                alphaMap.Alpha[y, x * 2] = (byte)((alphaByte >> 4) & 0xF); // Primeiros 4 bits
                alphaMap.Alpha[y, x * 2 + 1] = (byte)(alphaByte & 0xF); // Últimos 4 bits
            }
        }

        return alphaMap;
    }
}

// Estrutura para strings com comprimento prefixado
public struct LTString
{
    public ushort Length;
    public string Content;

    public static LTString Read(BinaryReader reader)
    {
        var ltString = new LTString
        {
            Length = reader.ReadUInt16(),
            Content = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadUInt16()))
        };
        return ltString;
    }
}

// Estrutura para cores (RGBA)
public struct Colour
{
    public byte Alpha;
    public byte Red;
    public byte Green;
    public byte Blue;

    public static Colour Read(BinaryReader reader)
    {
        return new Colour
        {
            Alpha = reader.ReadByte(),
            Red = reader.ReadByte(),
            Green = reader.ReadByte(),
            Blue = reader.ReadByte()
        };
    }
}

// Estrutura para o cabeçalho DTX
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

public enum Flags
{
    FullBrite=1,
    AlphaMasks=2,
    Unknown1=4,
    Unknown2=8
}

// Estrutura para pixels de um mipmap
public struct Pixels
{
    public byte[,] Data; // Matriz de pixels (índices de paleta)

    public static Pixels Read(BinaryReader reader, int width, int height)
    {
        var pixels = new Pixels
        {
            Data = new byte[width, height]
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                pixels.Data[x, y] = reader.ReadByte();
            }
        }

        return pixels;
    }
}
