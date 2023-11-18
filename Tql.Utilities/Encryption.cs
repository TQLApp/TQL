using System.Security.Cryptography;

namespace Tql.Utilities;

/// <summary>
/// Encryption utility methods.
/// </summary>
public static class Encryption
{
    /// <summary>
    /// SHA1 hashes the input.
    /// </summary>
    /// <param name="input">Input to hash.</param>
    /// <returns>Hash of the input.</returns>
    public static string Sha1Hash(string input)
    {
        using var sha1 = SHA1.Create();

        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

        return HexEncode(hash);
    }

    /// <summary>
    /// Hex encodes the data.
    /// </summary>
    /// <param name="data">Data to hex encode.</param>
    /// <returns>Hex encoded data.</returns>
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
