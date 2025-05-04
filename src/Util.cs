using System.Buffers.Binary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace dtx2png;

public static class Util
{
    public static int DivideByPowerOfTwo(int value, int power)
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
    
    public static void PrintDtxInfo(DtxFile dtxFile)
    {
        Print("Unknown1..........", dtxFile.Header.Unknown1);
        Print("Version...........", dtxFile.Header.Version);
        Print("Width.............", dtxFile.Header.Width);
        Print("Height............", dtxFile.Header.Height);
        Print("Mipmap Count......", dtxFile.Header.MipmapCount);
        Print("Has Lights........", dtxFile.Header.Fmt1);
        Print("Flags.............", dtxFile.Header.Fmt2);
        Print("Surface Flags.....", dtxFile.Header.SurfaceFlags);
        Print("Group.............", dtxFile.Header.Fmt3);
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
        Console.WriteLine(dtxFile.Header.Fmt1 != 0 && dtxFile.Lights != null
            ? $"{dtxFile.Lights.String.Length}"
            : "Not present.");

        Console.Write("Light Definitions: ");
        Console.WriteLine(dtxFile.Header.Fmt1 != 0 && dtxFile.Lights != null
            ? $"{dtxFile.Lights.LightDefs}"
            : "Not present.");
    }    

    public static Image FromColorArray(ColorBGRA[] colors, int width, int height)
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
    
}