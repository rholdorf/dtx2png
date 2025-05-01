using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace dtx2png;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dtx2png <input.dtx> [output.png]");
            return;
        }

        var outPath = args.Length < 2 ? Path.ChangeExtension(args[0], ".png") : args[1];

        using var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var dtxFile = DtxFile.Read(reader);
        PrintDtxInfo(dtxFile);
        if(!dtxFile.Header.Supported)
        {
            Console.WriteLine("Unsupported DTX format.");
            return;
        }

        var imageIndex = 0;
        foreach (var current in dtxFile.Colours)
        {
            var mipWidth = DivideByPowerOfTwo(dtxFile.Header.Width, imageIndex);
            var mipHeight = DivideByPowerOfTwo(dtxFile.Header.Height, imageIndex);

            using var img = FromColorArray(current, mipWidth, mipHeight);

            if (imageIndex == 0)
            {
                img.Save(outPath);
                Console.WriteLine($"Saved image to {outPath}");
            }
            else
            {
                var mipPath = Path.ChangeExtension(outPath, $"_mipmap_{imageIndex}.png");
                img.Save(mipPath);
                Console.WriteLine($"Saved mipmap {imageIndex} to {mipPath}");
            }
            
            imageIndex++;
        }
    }

    private static Image FromColorArray(ColorBGRA[] colors, int width, int height)
    {
        var ret = new Image<Rgba32>(width, height);
        var i = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = colors[i];
                ret[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
                i++;
            }
        }

        return ret;
    }
    
    private static int DivideByPowerOfTwo(int value, int power)
    {
        var ret = value >> power;
        if(ret <1)
            ret = 1;
        return ret;
    }

    private static void Print(string name)
    {
        Console.Write(name);
        Console.Write(": \t");        
    }
    private static void Print(string name, int value)
    {
        Print(name);
        Console.WriteLine($"{BinaryPrimitives.ReverseEndianness(value):X8} ({value})");
    }
    
    private static void Print(string name, ushort value)
    {
        Print(name);
        Console.WriteLine($"{BinaryPrimitives.ReverseEndianness(value):X4} ({value})");
    }    
    
    private static void Print(string name, uint value)
    {
        Print(name);
        Console.WriteLine($"{BinaryPrimitives.ReverseEndianness(value):X4} ({value})");
    }    
    
    private static void Print(string name, byte value)
    {
        Print(name);
        Console.WriteLine($"{BinaryPrimitives.ReverseEndianness(value):X2} ({value})");
    }    
    
    private static void PrintDtxInfo(DtxFile dtxFile)
    {
        Print("Unknown1..........", dtxFile.Header.Unknown1);
        Print("Version...........", dtxFile.Header.Version);
        Print("Width.............", dtxFile.Header.Width);
        Print("Height............", dtxFile.Header.Height);
        Print("Mipmap Count......", dtxFile.Header.MipmapCount);
        Print("Has Lights........", dtxFile.Header.HasLights);
        Print("Flags.............", dtxFile.Header.Flags);
        Print("Surface Flags.....", dtxFile.Header.SurfaceFlags);
        Print("Group.............", dtxFile.Header.Group);
        Print("Mipmaps Used Count", dtxFile.Header.MipmapsUsedCount);
        Print("Alpha Cutoff......", dtxFile.Header.AlphaCutoff);
        Print("Alpha Average.....", dtxFile.Header.AlphaAverage);
        Print("Unknown2..........", dtxFile.Header.Unknown2);
        Print("Unknown3..........", dtxFile.Header.Unknown3);
        Print("Unknown4..........", dtxFile.Header.Unknown4);
        Print("Unknown5..........", dtxFile.Header.Unknown5);
        Print("Unknown6..........", dtxFile.Header.Unknown6);
        Print("Unknown7..........", dtxFile.Header.Unknown7);
        Print("Unknown8..........", dtxFile.Header.Unknown8);
        Print("Unknown9..........", dtxFile.Header.Unknown9);
        
        //Console.WriteLine($"\tFullBright: {dtxFile.Header.FullBright}");
        //Console.WriteLine($"\tAlpha Maks: {dtxFile.Header.AlphaMasks}");
        //Console.WriteLine($"\tFlag Unknown1: {dtxFile.Header.FlagUnknown1}");
        //Console.WriteLine($"\tFlag Unknown2: {dtxFile.Header.FlagUnknown2}");
        //Console.WriteLine($"\tFlag Unknown3: {dtxFile.Header.FlagUnknown3}");
        //Console.WriteLine($"\tMap to master palette: {dtxFile.Header.MapToMasterPalette}");
        //Console.Write("Color Palette Size: ");
        //Console.WriteLine(dtxFile.Colours != null ? $"{dtxFile.Colours.Length}" : "Not present.");
        Console.Write("Lights Count: ");
        Console.WriteLine(dtxFile.Header.HasLights != 0 && dtxFile.Lights != null
            ? $"{dtxFile.Lights.String.Length}"
            : "Not present.");

        Console.Write("Light Definitions: ");
        Console.WriteLine(dtxFile.Header.HasLights != 0 && dtxFile.Lights != null
            ? $"{dtxFile.Lights.LightDefs}"
            : "Not present.");
    }
}