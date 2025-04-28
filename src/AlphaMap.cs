public struct AlphaMap
{
    public byte[,] Alpha; // Matriz de valores de alfa (4 bits por pixel)

    public static AlphaMap Read(BinaryReader reader, int width, int height)
    {
        var alphaMap = new AlphaMap
        {
            Alpha = new byte[height, width]
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width / 2; x++) // Cada byte contém dois valores de alfa (4 bits cada)
            {
                var alphaByte = reader.ReadByte();
                alphaMap.Alpha[y, x * 2] = (byte)((alphaByte >> 4) & 0xF); // Primeiros 4 bits
                alphaMap.Alpha[y, x * 2 + 1] = (byte)(alphaByte & 0xF); // Últimos 4 bits
            }
        }

        return alphaMap;
    }
}