using System.Security.Cryptography;

namespace Tql.Plugins.Jira.Support;

internal static class Encryption
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

    public static string Hash(string input)
    {
        using var sha1 = SHA1.Create();

        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(hash.Length * 2);

        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
