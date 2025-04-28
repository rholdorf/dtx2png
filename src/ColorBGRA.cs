public struct ColorBGRA
{
    public byte Alpha;
    public byte Red;
    public byte Green;
    public byte Blue;

    public ColorBGRA(byte alpha, byte red, byte green, byte blue)
    {
        Alpha = alpha;
        Red = red;
        Green = green;
        Blue = blue;
    }
    public ColorBGRA()
    {
    }
    
    public static ColorBGRA Read(BinaryReader reader)
    {
        return new ColorBGRA
        {
            Blue = reader.ReadByte(),
            Green = reader.ReadByte(),
            Red = reader.ReadByte(),
            Alpha = reader.ReadByte()
        };
    }
}