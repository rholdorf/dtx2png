using System.Diagnostics;

namespace dtx2png;

public class DtxFile
{
    public Header Header { get; }
    public List<Texture>? Textures { get; }

    public DtxFile(BinaryReader reader)
    {
        Header = Header.Read(reader);
        Textures = new List<Texture>(Header.MipmapCount);
        if (Header.Fmt2 == 136)
        {
            Textures.Add(ReadColorData(reader, Header.Width, Header.Height, TextureFormat.RGBA));
        }
        else if(Header.Fmt2 == 8)
        {
            reader.BaseStream.Seek(4, SeekOrigin.Current);
            Textures.Add(ReadColorData(reader, Header.Width, Header.Height, TextureFormat.BGRA));
        }
    }

    private static Texture ReadColorData(BinaryReader reader, int width, int height, TextureFormat textureFormat)
    {
        var texture = new Texture
        {
            Width = width,
            Height = height,
            Format = textureFormat
        };

        var size = height * width;
        if (textureFormat == TextureFormat.RGBA || textureFormat == TextureFormat.BGRA)
            size *= 4;
        
        texture.PixelData = new byte[size];
        for (var i = 0; i < size; i++)
        {
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                texture.PixelData[i] = Util.SafeReadByte(reader);
            }
            else
            {
                texture.PixelData[i] = 0x00;
            }
        }

        return texture;
    }
}