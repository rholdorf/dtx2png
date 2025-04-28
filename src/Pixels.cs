namespace dtx2png;

public struct Pixels
{
    public byte[,] Data; // Matriz de pixels (índices de paleta)

    public static Pixels Read(BinaryReader reader, int width, int height)
    {
        var pixels = new Pixels
        {
            Data = new byte[width, height]
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                pixels.Data[x, y] = reader.ReadByte();
            }
        }

        return pixels;
    }
}