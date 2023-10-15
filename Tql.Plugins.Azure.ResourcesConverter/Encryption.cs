﻿using System.Security.Cryptography;
using System.Text;

namespace Tql.Plugins.Azure.ResourcesConverter;

internal static class Encryption
{
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