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
