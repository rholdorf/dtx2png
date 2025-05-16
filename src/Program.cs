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

        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"{args[0]} not found.");
            return;
        }

        try
        {
            Do2(args);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERR: ");
            Console.ResetColor();
            Console.WriteLine(e);
        }
    }

    private static void Do2(string[] args)
    {
        var outPath = args.Length < 2 ? Path.ChangeExtension(args[0], ".png") : args[1];

        using var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var dtxFile = new Dtx();
        using var img = dtxFile.Read(reader);
        if (img is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERR: {args[0]} - no image to save.");
            Console.ResetColor();
            return;
        }
        
        img.Save(outPath);
        Console.WriteLine($"INF: Saved image to {outPath}");
    }    

    private static void Do1(string[] args)
    {
        var outPath = args.Length < 2 ? Path.ChangeExtension(args[0], ".png") : args[1];

        using var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var dtxFile = new DtxFile(reader);
        Util.PrintDtxInfo(dtxFile);
        if (!dtxFile.Header.Supported)
        {
            Console.WriteLine("ERR: Unsupported DTX format.");
            return;
        }

        if (dtxFile.Textures?.Count == 0)
        {
            Console.WriteLine("ERR: No dtx textures found.");
            return;
        }
        
        var imageIndex = 0;
        foreach (var current in dtxFile.Textures)
        {
            using var img = Util.FromTexture(current);

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
}