using System.IO;
using System.Security.Cryptography;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class Encryption : IEncryption, IDisposable
{
    private readonly Aes _aes;

    public Encryption(Settings settings)
    {
        _aes = EnsureEncryptionKey(settings);
    }

    private Aes EnsureEncryptionKey(Settings settings)
    {
        var aes = Aes.Create();

        aes.Padding = PaddingMode.PKCS7;

        var json = settings.EncryptionKey;
        if (json != null)
        {
            var key = JsonSerializer.Deserialize<KeyDto>(
                Unprotect(Convert.FromBase64String(json))
            )!;

            aes.Key = key.Key;
            aes.IV = key.IV;
        }
        else
        {
            var key = new KeyDto(aes.Key, aes.IV);

            settings.EncryptionKey = Convert.ToBase64String(Protect(JsonSerializer.Serialize(key)));
        }

        return aes;
    }

    public string Encrypt(byte[] data)
    {
        return Encrypt(data, 0, data.Length);
    }

    public string Encrypt(byte[] data, int offset, int length)
    {
        using var encryptor = _aes.CreateEncryptor();

        using var target = new MemoryStream();

        using (var source = new CryptoStream(target, encryptor, CryptoStreamMode.Write))
        {
            source.Write(data, offset, length);
        }

        return Convert.ToBase64String(target.ToArray());
    }

    public string? EncryptString(string? data)
    {
        if (data == null)
            return null;

        return Encrypt(Encoding.UTF8.GetBytes(data));
    }

    public byte[] Decrypt(string encrypted)
    {
        var bytes = Convert.FromBase64String(encrypted);

        using var decryptor = _aes.CreateDecryptor();

        using var source = new MemoryStream(bytes);
        using var target = new CryptoStream(source, decryptor, CryptoStreamMode.Read);
        using var stream = new MemoryStream();

        target.CopyTo(stream);

        return stream.ToArray();
    }

    public string? DecryptString(string? encrypted)
    {
        if (encrypted == null)
            return null;

        return Encoding.UTF8.GetString(Decrypt(encrypted));
    }

    private string Unprotect(byte[] value)
    {
        return Encoding.UTF8.GetString(
            ProtectedData.Unprotect(value, null, DataProtectionScope.CurrentUser)
        );
    }

    private byte[] Protect(string value)
    {
        return ProtectedData.Protect(
            Encoding.UTF8.GetBytes(value),
            null,
            DataProtectionScope.CurrentUser
        );
    }

    public void Dispose()
    {
        _aes.Dispose();
    }

    private record KeyDto(byte[] Key, byte[] IV);
}
