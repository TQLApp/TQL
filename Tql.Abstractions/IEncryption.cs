namespace Tql.Abstractions;

public interface IEncryption
{
    string Encrypt(byte[] data);
    string Encrypt(byte[] data, int offset, int length);
    string? EncryptString(string? data);

    byte[] Decrypt(string encrypted);
    string? DecryptString(string? encrypted);
}
