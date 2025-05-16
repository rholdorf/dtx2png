namespace dtx2png;

public class Texture
{
    public int Width { get; set;  } = 0;
    public int Height { get; set; } = 0;
    public byte[] PixelData { get; set; } = [];
    public TextureFormat Format { get; set; } = TextureFormat.Unknown;
}