namespace Tql.Abstractions;

/// <summary>
/// Represents the encryption service to encrypt and decrypt user data.
/// </summary>
/// <remarks>
/// <para>
/// The encryption service provides methods to encrypt and decrypt user
/// data. The intended purpose for this method is to store user credentials
/// in a safe manner.
/// </para>
///
/// <para>
/// The encryption service uses an encryption key managed by TQL. The
/// encryption key is stored in the users account in a safe manner using
/// <a href="https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata">ProtectedData</a>.
/// The reason for this indirection is that this allows us to sync data
/// across user devices by synching the encryption key with the users data.
/// </para>
/// </remarks>
public interface IEncryption
{
    /// <summary>
    /// Encrypt the provided data.
    /// </summary>
    /// <param name="data">Data to encrypt.</param>
    /// <returns>Encrypted data.</returns>
    string Encrypt(byte[] data);

    /// <summary>
    /// Encrypt the provided data.
    /// </summary>
    /// <param name="data">Data to encrypt.</param>
    /// <param name="offset">Offset into the data.</param>
    /// <param name="length">Length of the data to encrypt.</param>
    /// <returns>Encrypted data.</returns>
    string Encrypt(byte[] data, int offset, int length);

    /// <summary>
    /// Encrypt the provided data.
    /// </summary>
    /// <param name="data">Data to encrypt.</param>
    /// <returns>Encrypted data.</returns>
    string? EncryptString(string? data);

    /// <summary>
    /// Decrypt the encrypted data.
    /// </summary>
    /// <param name="encrypted">Encrypted data.</param>
    /// <returns>Decrypted data.</returns>
    byte[] Decrypt(string encrypted);

    /// <summary>
    /// Decrypt the encrypted data.
    /// </summary>
    /// <param name="encrypted">Encrypted data.</param>
    /// <returns>Decrypted data.</returns>
    string? DecryptString(string? encrypted);
}
