using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Formats.Png;

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
            Error($"{args[0]} not found.");
            return;
        }

        using var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        if (stream.Length == 0)
        {
            Error($"{args[0]} is empty.");
            return;
        }

        try
        {
            using var reader = new BinaryReader(stream);
            using var dtxFile = new Dtx();
            if (dtxFile.Read(reader))
            {
                var outPath = args.Length < 2 ? Path.ChangeExtension(args[0], null) : args[1];
                SaveImagesAsPng(dtxFile, outPath);                
            }
            else
            {
                Error(dtxFile.Error);
            }
        }
        catch (Exception e)
        {
            Error(e.ToString());
        }
    }

    private static void Error(string msg)
    {
        Console.Error.Write($"ERR: ");
        Console.Error.WriteLine(msg);
    }
    
    private static void SaveImagesAsPng(Dtx dtxFile, string outPath)
    {
        for (var i = 0; i < dtxFile.Images.Count; i++)
        {
            var image = dtxFile.Images[i];
            var filename = i == 0 ? $"{outPath}.png" : $"{outPath}_{i}.png";
            image.Save(filename, new PngEncoder());
            Console.WriteLine($"INF: Saved image to {filename}");
        }
    }
}