using System.Text;

public class Lights
{
    public string LightDefs; // Dados de luz (tamanho variável)
    public byte[] Unknown; // 18 bytes desconhecidos
    public LTLongString String;

    public static Lights Read(BinaryReader reader)
    {
        var lights = new Lights
        {
            // LIGHTDEFS é uma string de tamanho variável, não especificada no formato
            // Vamos assumir que é uma string terminada em null ou de tamanho fixo
            // Aqui, você pode precisar ajustar com base em engenharia reversa
            LightDefs = ReadNullTerminatedString(reader),
            Unknown = reader.ReadBytes(18),
            String = LTLongString.Read(reader)
        };
        return lights;
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var sb = new StringBuilder();
        byte b;
        while ((b = reader.ReadByte()) != 0)
        {
            sb.Append((char)b);
        }
        return sb.ToString();
    }
}