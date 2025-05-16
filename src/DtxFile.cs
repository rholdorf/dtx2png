using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace dtx2png;

public class DtxFile
{
    private const int DTX5_BLOCK_BYTES = 16;
    private const int DTX1_BLOCK_BYTES = 8;
    private const int HEADER_SIZE = 52;

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
        else if (Header.Fmt2 == 8)
        {
            if (Header.Fmt3 == 0)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                for (var i = 0; i < Header.MipmapCount; i++)
                {
                    Textures.Add(ReadColorData(reader, Header.Width, Header.Height, TextureFormat.BGRA));
                }
            }
            else if (Header.Fmt3 == 1)
            {
                reader.BaseStream.Seek(HEADER_SIZE, SeekOrigin.Begin);
                for (var i = 0; i < Header.MipmapCount; i++)
                {
                    Textures.Add(ReadColorData(reader, Header.Width, Header.Height, TextureFormat.DXT1));
                }
            }
        }
        else
        {
            Console.WriteLine("INF: Unknown format.");
        }

    }

    private static Texture ReadColorData(BinaryReader reader, int width, int height, TextureFormat textureFormat)
    {
        var size = width * height;
        if (textureFormat == TextureFormat.RGBA || textureFormat == TextureFormat.BGRA)
        {
            return ReadRaw(size, reader, width, height, textureFormat);
        }
        
        if (textureFormat == TextureFormat.DXT1)
        {
            return ReadEncoded(reader, width, height, textureFormat);
        }

        return new Texture() { Width = width, Height = height, Format = TextureFormat.Unknown };
    }

    private static Texture ReadRaw(int size, BinaryReader reader, int width, int height, TextureFormat textureFormat)
    {
        var texture = new Texture
        {
            Width = width,
            Height = height,
            Format = textureFormat
        };
        
        if (texture.Format is TextureFormat.RGBA or TextureFormat.BGRA)
            size *= 4;

        texture.PixelData = new byte[size];
        for (var i = 0; i < size; i++)
        {
            texture.PixelData[i] = Util.SafeReadByte(reader);
        }

        return texture;
    }

    private static Texture ReadEncoded(BinaryReader reader, int width, int height, TextureFormat textureFormat)
    {
        var texture = new Texture
        {
            Width = width,
            Height = height,
            Format = textureFormat
        };
        
        // Determine block size and total compressed data length for MIP0
        var blockBytes = (texture.Format == TextureFormat.DXT5 ? DTX5_BLOCK_BYTES : DTX1_BLOCK_BYTES);
        var blocksX = (texture.Width + 3) / 4;
        var blocksY = (texture.Height + 3) / 4;
        var mipSize = blocksX * blocksY * blockBytes;

        // Read compressed blocks
        var compressedBlocks = reader.ReadBytes(mipSize);

        // Decode the entire image using BCnEncoder
        var decoder = new BcDecoder();
        var format = (blockBytes == DTX5_BLOCK_BYTES)
            ? CompressionFormat.Bc3
            : CompressionFormat.Bc1;

        var blockSize = decoder.GetBlockSize(format);

        var buffer = new List<byte>();
        for (var i = 0; i < compressedBlocks.Length; i += blockSize)
        {
            var encodedBlock = compressedBlocks.AsSpan(i, blockSize);
            var currentDecodedBlock = decoder.DecodeBlock(encodedBlock, format).Span;

            for (var j = 0; j < currentDecodedBlock.Height; j++)
            {
                for (var k = 0; k < currentDecodedBlock.Width; k++)
                {
                    var current = currentDecodedBlock[new Index(j), new Index(k)];
                    buffer.Add(current.r);
                    buffer.Add(current.g);
                    buffer.Add(current.b);
                    buffer.Add(current.a);
                }
            }
        }

        texture.PixelData = buffer.ToArray();

        // Load pixel data into an ImageSharp image
        //using Image<Rgba32> decodedImage = Image.LoadPixelData<Rgba32>(rawPixelData, texture.Width, texture.Height);


        // Copy pixel data into the texture
        //texture.PixelData = new byte[texture.Width * texture.Height * 4];
        //decodedImage.CopyPixelDataTo(texture.PixelData);

        return texture;
    }
}