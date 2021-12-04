namespace Woof.DataProtection;

/// <summary>
/// Defines a common interface for the cross platform data protection API.
/// </summary>
public interface IDPAPI {

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Protected data.</returns>
    byte[] Protect(byte[] data, DataProtectionScope scope = default);

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted data.</returns>
    byte[] Unprotect(byte[] data, DataProtectionScope scope = default);

}