public class DtxFile
{
    public Header Header;
    public ColorBGRA[] Colours;
    public Pixels[] Mipmaps;
    public AlphaMap[] AlphaMaps;
    public Lights? Lights;
    public ColorBGRA[] PixelData;

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
            dtxFile.PixelData = new ColorBGRA[dtxFile.Header.Width * dtxFile.Header.Height];
            for (var i = 0; i < dtxFile.PixelData.Length; i++)
            {
                dtxFile.PixelData[i] = ColorBGRA.Read(reader);
            }
        }
        else
        {
            // Leitura da paleta de cores (sempre 256 cores)
            dtxFile.Colours = new ColorBGRA[256];
            for (var i = 0; i < 256; i++)
            {
                dtxFile.Colours[i] = ColorBGRA.Read(reader);
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