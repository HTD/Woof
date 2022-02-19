using System.Security.Principal;

namespace Woof.DataProtection.Api;

#pragma warning disable CA1416 // System.Security.Cryptography.ProtectedData is used internally for Windows only.

/// <summary>
/// Windows DPAPI translator.
/// </summary>
public class DPAPI : IDPAPI {

    /// <summary>
    /// Gets or sets a value indicating that the service API should be used for the <see cref="DataProtectionScope.LocalMachine"/> scope.
    /// </summary>
    public static bool UseServiceAPI {
        get => _UseServiceAPI;
        set {
            using var currentIdentity = WindowsIdentity.GetCurrent();
            var isAdmin = new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator);
            if (value && !isAdmin) throw new InvalidOperationException("The service API can be set only by the Administrator.");
            _UseServiceAPI = value;
            if (ServiceAPI is not null) return;
            ServiceAPI = new WindowsLocalSystemKey();
        }
    }

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Protected data.</returns>
    public byte[] Protect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => scope switch {
            DataProtectionScope.CurrentUser => ProtectedData.Protect(data, null, scope.AsSystemType()),
            DataProtectionScope.LocalMachine => ServiceAPI is null
                ? ProtectedData.Protect(data, null, scope.AsSystemType())
                : ServiceAPI.Protector.Protect(data),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">One of the enumeration values that specifies the scope of encryption.</param>
    /// <returns>Decrypted data.</returns>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => scope switch {
            DataProtectionScope.CurrentUser => ProtectedData.Unprotect(data, null, scope.AsSystemType()),
            DataProtectionScope.LocalMachine => ServiceAPI is null
                ? ProtectedData.Unprotect(data, null, scope.AsSystemType())
                : ServiceAPI.Protector.Unprotect(data),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };

    /// <summary>
    /// Gets a specific DPAPI instance if available.
    /// </summary>
    /// <typeparam name="T">Specific DPAPI implementation.</typeparam>
    /// <returns>Specific DPAPI implementation.</returns>
    public static T? GetInstance<T>() where T : class, IDPAPI => DP.DPAPI as T;

    /// <summary>
    /// <see cref="WindowsLocalSystemKey"/> instance used as a special service API.
    /// </summary>
    private static IDataProtectionKey? ServiceAPI;

    /// <summary>
    /// <see cref="UseServiceAPI"/> backing field.
    /// </summary>
    private static bool _UseServiceAPI;

}

#pragma warning restore CA1416