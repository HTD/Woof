namespace Woof.DataProtection;

/// <summary>
/// A minimalistic, multi-platform, static data protection module.
/// </summary>
public static class DP {

    /// <summary>
    /// Gets the current data protection API.
    /// </summary>
    public static IDPAPI DPAPI { get; }

    /// <summary>
    /// Gets a value indicating that the data protection service is operational.<br/>
    /// It always returns true on Windows, because Protect and Unprotect mehtods for windows don't require special configuration.<br/>
    /// If false is returned, it indicates the program doesn't have sufficient access to the key directory.<br/>
    /// This happens when the key directory and key file cannot be read by the program.<br/>
    /// This also happens when the program is first run, the key file does not exist and the program can't write to the key directory.
    /// It will also return false if you forget to add a reference for the non-windows OS extension package.
    /// </summary>
    public static bool IsAvailable => DPAPI.IsOperational;

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Protected data.</returns>
    public static byte[] Protect(byte[] data, DataProtectionScope scope = default) => DPAPI.Protect(data, scope);

    /// <summary>
    /// Encrypts a string.
    /// </summary>
    /// <param name="plaintext">A string to encrypt.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption..</param>
    /// <returns>Encrypted string.</returns>
    public static string Protect(string plaintext, DataProtectionScope scope = default) {
        var data = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = DPAPI.Protect(data, scope);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted data.</returns>
    public static byte[] Unprotect(byte[] data, DataProtectionScope scope = default) => DPAPI.Unprotect(data, scope);
    /// <summary>
    /// Decrypts a string.
    /// </summary>
    /// <param name="protectedText">A string to decrypt.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted string.</returns>
    public static string Unprotect(string protectedText, DataProtectionScope scope = default) {
        var data = Convert.FromBase64String(protectedText);
        var decrypted = DPAPI.Unprotect(data, scope);
        return Encoding.UTF8.GetString(decrypted);
    }

    /// <summary>
    /// Configures the data protection API for the current OS.
    /// </summary>
    static DP()
        => DPAPI = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new Api.DPAPI()
            : ApiResolver.GetNonWindowsDPAPI<IDPAPI>() ?? new Api.Unsupported();

}