using System.Security.Cryptography;

namespace Tql.App.Support;

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
}
