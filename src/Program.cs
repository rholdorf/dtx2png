using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
        
        try
        {
            using var stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
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
                    var outputPath = Path.ChangeExtension(args[0], $"_mipmap_{i}.png");

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
        catch (Exception e)
        {
            Console.WriteLine($"ERR - File: {args[0]} - {e.Message}");
        }
    }

    private static void PrintDtxInfo(DtxFile dtxFile)
    {
        Console.WriteLine($"Unknown1: {dtxFile.Header.Unknown1}");
        Console.WriteLine($"Version: {dtxFile.Header.Version}");
        Console.WriteLine($"Width: {dtxFile.Header.Width}, Height: {dtxFile.Header.Height}");
        Console.WriteLine($"Mipmap Count: {dtxFile.Header.MipmapCount}");
        Console.WriteLine($"Has Lights: {dtxFile.Header.HasLights != 0}");
        Console.WriteLine($"Flags: {dtxFile.Header.Flags.ToString()}");
        Console.WriteLine($"\tFullBright: {dtxFile.Header.FullBright}");
        Console.WriteLine($"\tAlpha Maks: {dtxFile.Header.AlphaMasks}");
        Console.WriteLine($"\tFlag Unknown1: {dtxFile.Header.FlagUnknown1}");
        Console.WriteLine($"\tFlag Unknown2: {dtxFile.Header.FlagUnknown2}");
        Console.WriteLine($"\tFlag Unknown3: {dtxFile.Header.FlagUnknown3}");
        Console.WriteLine($"\tMap to master palette: {dtxFile.Header.MapToMasterPalette}");
        Console.WriteLine($"Surface Flags: 0x{dtxFile.Header.SurfaceFlags:X}");
        Console.WriteLine($"Group: {dtxFile.Header.Group}");
        Console.WriteLine($"Mipmaps Used Count: {dtxFile.Header.MipmapsUsedCount}");
        Console.WriteLine($"Alpha Cutoff: {dtxFile.Header.AlphaCutoff}");
        Console.WriteLine($"Alpha Average: {dtxFile.Header.AlphaAverage}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown2}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown3}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown4}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown5}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown6}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown7}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown8}");
        Console.WriteLine($"Unknown2: {dtxFile.Header.Unknown9}");
        
        Console.Write("Color Palette Size: ");
        Console.WriteLine(dtxFile.Colours != null ? $"{dtxFile.Colours.Length}" : "Not present.");
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