namespace Woof.DataProtection.Api;

#pragma warning disable CA1416 // System.Security.Cryptography.ProtectedData is used internally for Windows only.

/// <summary>
/// Windows DPAPI translator.
/// </summary>
public class DPAPI : IDPAPI {

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Protected data.</returns>
    public byte[] Protect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => ProtectedData.Protect(data, null, scope.AsSystemType());

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted data.</returns>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => ProtectedData.Unprotect(data, null, scope.AsSystemType());

    /// <summary>
    /// Gets a specific DPAPI instance if available.
    /// </summary>
    /// <typeparam name="T">Specific DPAPI implementation.</typeparam>
    /// <returns>Specific DPAPI implementation.</returns>
    public static T? GetInstance<T>() where T : class, IDPAPI => DP.DPAPI as T;

}

#pragma warning restore CA1416