using System.Diagnostics;

namespace dtx2png;

public class DtxFile
{
    public Header Header;
    public List<ColorBGRA[]>? Colours;
    public Pixels[]? Mipmaps;
    public AlphaMap[]? AlphaMaps;
    public Lights? Lights;


    public static DtxFile Read(BinaryReader reader)
    {

        var dtxFile = new DtxFile
        {
            Header = Header.Read(reader)
        };

        dtxFile.Colours = new List<ColorBGRA[]>(dtxFile.Header.MipmapCount);
        
        reader.BaseStream.Seek(120, SeekOrigin.Current); // TODO: Why 120? What are these skipped bytes?            
        if (dtxFile.Header.Flags == 8)
        {
            dtxFile.Colours.Add(ReadColorData(reader, dtxFile.Header.Width, dtxFile.Header.Height));
        }
        else
        {
            reader.BaseStream.Seek(682 - 120 - 40, SeekOrigin.Begin);

            // Leitura da paleta de cores (sempre 256 cores)
            reader.BaseStream.Seek(256*4, SeekOrigin.Current);
            
            //dtxFile.Colours = new ColorBGRA[256];
            //for (var i = 0; i < 256; i++)
            //{
                //dtxFile.Colours[i] = ColorBGRA.Read(reader);
            //}

            // Leitura dos mipmaps
            for (var i = 0; i < dtxFile.Header.MipmapCount; i++)
            {
                var mipWidth = dtxFile.Header.Width >> i;
                var mipHeight = dtxFile.Header.Height >> i;
                if (mipWidth < 1) mipWidth = 1;
                if (mipHeight < 1) mipHeight = 1;

                dtxFile.Colours.Add(ReadColorData(reader, mipWidth, mipHeight));
            }

            // // Leitura dos mapas de alfa (se DTX_ALPHA_MASKS estiver definido)
            // if (dtxFile.Header.Flags == 8) // DTX_ALPHA_MASKS
            // {
            //     dtxFile.AlphaMaps = new AlphaMap[dtxFile.Header.MipmapCount];
            //     for (var i = 0; i < dtxFile.Header.MipmapCount; i++)
            //     {
            //         var mipWidth = dtxFile.Header.Width >> i;
            //         var mipHeight = dtxFile.Header.Height >> i;
            //         if (mipWidth < 1) mipWidth = 1;
            //         if (mipHeight < 1) mipHeight = 1;
            //         dtxFile.AlphaMaps[i] = AlphaMap.Read(reader, mipWidth, mipHeight);
            //     }
            // }

            // Leitura dos dados de luz (se has_lights for verdadeiro)
            //if (dtxFile.Header.HasLights != 0)
            //{
            //    dtxFile.Lights = Lights.Read(reader);
            //}
        }

        return dtxFile;
    }

    private static ColorBGRA[] ReadColorData(BinaryReader reader, int width, int height)
    {
        var colors = new ColorBGRA[width * height];
        for (var i = 0; i < colors.Length; i++)
        {
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                colors[i] = ColorBGRA.Read(reader);
            }
            else
            {
                colors[i] = new ColorBGRA(0, 0, 0, 0);
            }
        }

        return colors;
    }
}