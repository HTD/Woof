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
        => AssertValidWindowsScope(scope) switch {
            DataProtectionScope.CurrentUser or DataProtectionScope.LocalMachine => ProtectedData.Protect(data, null, scope.AsSystemType()),
            DataProtectionScope.LocalSystem => ServiceAPI.Protector.Protect(data),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted data.</returns>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => AssertValidWindowsScope(scope) switch {
            DataProtectionScope.CurrentUser or DataProtectionScope.LocalMachine => ProtectedData.Unprotect(data, null, scope.AsSystemType()),
            DataProtectionScope.LocalSystem => ServiceAPI.Protector.Unprotect(data),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };

    /// <summary>
    /// When called on windows, it will throw an exception if the data protection scope is set for LOCAL SYSTEM
    /// and the current principal is not in the Administrator role.
    /// </summary>
    /// <param name="scope">Data protection scope.</param>
    /// <returns>The <paramref name="scope"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Data protection scope is <see cref="DataProtectionScope.LocalSystem"/>
    /// and the current principal is not in the Administrator role.
    /// </exception>
    public static DataProtectionScope AssertValidWindowsScope(DataProtectionScope scope) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || scope != DataProtectionScope.LocalSystem) return scope;
        using var currentIdentity = WindowsIdentity.GetCurrent();
        var isAdmin = new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator);
        return isAdmin ? scope : throw new InvalidOperationException("The service API can be set only by the Administrator.");
    }

    /// <summary>
    /// Gets a specific DPAPI instance if available.
    /// </summary>
    /// <typeparam name="T">Specific DPAPI implementation.</typeparam>
    /// <returns>Specific DPAPI implementation.</returns>
    public static T? GetInstance<T>() where T : class, IDPAPI => DP.DPAPI as T;

    /// <summary>
    /// <see cref="WindowsLocalSystemKey"/> instance used as a special service API.
    /// </summary>
    private static IDataProtectionKey ServiceAPI => _ServiceAPI ??= new WindowsLocalSystemKey();

    /// <summary>
    /// A backing field for <see cref="ServiceAPI"/> singleton.
    /// </summary>
    private static IDataProtectionKey? _ServiceAPI;

}

#pragma warning restore CA1416