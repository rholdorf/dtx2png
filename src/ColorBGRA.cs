using System.Diagnostics;

namespace dtx2png;

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

            Blue = SafeReadByte(reader),
            Green = SafeReadByte(reader),
            Red = SafeReadByte(reader),
            Alpha = SafeReadByte(reader)
        };

    }

    private static byte SafeReadByte(BinaryReader reader)
    {
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            return reader.ReadByte();
        }
        Debug.WriteLine("EOF reached while reading byte.");
        return 0x00;
    }
}