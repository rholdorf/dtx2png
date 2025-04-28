using System.Text;

namespace dtx2png;

public struct LTLongString
{
    public uint Length;
    public string Content;

    public static LTLongString Read(BinaryReader reader)
    {
        var ltLongString = new LTLongString
        {
            Length = reader.ReadUInt32(),
            Content = Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32()))
        };
        return ltLongString;
    }
}