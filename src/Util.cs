using System.Buffers.Binary;
using System.Diagnostics;
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
        Print("Fmt1..............", dtxFile.Header.Fmt1);
        Print("Fmt2..............", dtxFile.Header.Fmt2);
        Print("Surface Flags.....", dtxFile.Header.SurfaceFlags);
        Print("Fmt3..............", dtxFile.Header.Fmt3);
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
    }    

    public static Image FromTexture(Texture texture)
    {
        var ret = new Image<Rgba32>(texture.Width, texture.Height);
        var i = 0;
        byte red, green, blue, alpha;
        for (var y = 0; y < texture.Height; y++)
        {
            for (var x = 0; x < texture.Width; x++)
            {
                if (texture.Format == TextureFormat.BGRA)
                {
                    blue = texture.PixelData[i++];
                    green = texture.PixelData[i++];
                    red = texture.PixelData[i++];
                    alpha = texture.PixelData[i++];
                }
                else
                {
                    red = texture.PixelData[i++];
                    green = texture.PixelData[i++];
                    blue = texture.PixelData[i++];
                    alpha = texture.PixelData[i++];
                }

                ret[x, y] = new Rgba32(red, green, blue, alpha);
            }
        }

        return ret;
    }    
    
    
    public static byte SafeReadByte(BinaryReader reader)
    {
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            return reader.ReadByte();
        }
        Debug.WriteLine("EOF reached while reading byte.");
        return 0x00;
    }
}