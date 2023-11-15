using System.Security.Cryptography;

namespace Tql.Utilities;

public static class Encryption
{
    public static string Sha1Hash(string input)
    {
        using var sha1 = SHA1.Create();

        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

        return HexEncode(hash);
    }

    public static string HexEncode(byte[] data)
    {
        var sb = new StringBuilder(data.Length * 2);

        foreach (byte b in data)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
