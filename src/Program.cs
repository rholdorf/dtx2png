using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace dtx2png;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: dtx2png <input.dtx> <output.png>");
            return;
        }

        var inputPath = args[0];
        var outPath = args[1];

        using var stream = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);

        var dtxFile = DtxFile.Read(reader);
        PrintDtxInfo(dtxFile);

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
            Console.WriteLine($"Saved image to {outPath}");
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
                        var index = currentMipmap.Data[x, y];
                        var color = dtxFile.Colours[index];
                        img[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
                    }
                }

                img.Save(outputPath);
                Console.WriteLine($"Saved mipmap {i} to {outputPath}");
            }
        }
    }

    private static void PrintDtxInfo(DtxFile dtxFile)
    {
        Console.WriteLine($"Width: {dtxFile.Header.Width}, Height: {dtxFile.Header.Height}");
        Console.WriteLine($"Mipmap Count: {dtxFile.Header.MipmapCount}");
        Console.WriteLine($"Flags: 0x{dtxFile.Header.Flags:X}");
        Console.WriteLine($"Surface Flags: 0x{dtxFile.Header.SurfaceFlags:X}");
        Console.WriteLine($"Has Lights: {dtxFile.Header.HasLights != 0}");
        
        Console.Write("Color Palette Size: ");
        Console.WriteLine(dtxFile.Colours != null ? $"{dtxFile.Colours.Length}" : "Not present.");

        Console.Write("Mipmaps Count: ");
        Console.WriteLine(dtxFile.Mipmaps != null ? $"{dtxFile.Mipmaps?.Length}" : "Not present.");
        
        Console.Write("Alpha Maps Count: ");
        Console.WriteLine(dtxFile.AlphaMaps != null ? $"{dtxFile.AlphaMaps.Length}" : "Not present.");

        Console.Write("Lights Count: ");
        Console.WriteLine(dtxFile.Header.HasLights != 0 && dtxFile.Lights != null ? $"{dtxFile.Lights.String.Length}" : "Not present.");

        Console.Write("Light Definitions: ");
        Console.WriteLine(dtxFile.Header.HasLights != 0 && dtxFile.Lights != null ? $"{dtxFile.Lights.LightDefs}" : "Not present.");
    }
}