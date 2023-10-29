using System.Security.Cryptography;
using System.Text;

namespace Tql.Utilities;

public static class Encryption
{
    public static string? Unprotect(byte[]? value)
    {
        if (value == null)
            return null;

        try
        {
            return Encoding.UTF8.GetString(
                ProtectedData.Unprotect(value, null, DataProtectionScope.CurrentUser)
            );
        }
        catch
        {
            return null;
        }
    }

    public static byte[]? Protect(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return ProtectedData.Protect(
            Encoding.UTF8.GetBytes(value),
            null,
            DataProtectionScope.CurrentUser
        );
    }

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
