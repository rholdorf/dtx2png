using System.Text;

public struct LTString
{
    public ushort Length;
    public string Content;

    public static LTString Read(BinaryReader reader)
    {
        var ltString = new LTString
        {
            Length = reader.ReadUInt16(),
            Content = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadUInt16()))
        };
        return ltString;
    }
}