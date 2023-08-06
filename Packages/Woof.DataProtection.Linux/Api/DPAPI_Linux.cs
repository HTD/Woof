namespace Woof.DataProtection.Api;

/// <summary>
/// Linux data protection API.
/// </summary>
public class DPAPI_Linux : IDPAPI, IAcceptMessage {

    /// <summary>
    /// Accepts a message requesting keys configuration for a specified system user.
    /// </summary>
    /// <remarks>
    /// Called by SystemDServiceInstaller.RegisterSystemDServiceAsync() to configure the data protection if the service uses it.<br/>
    /// When this message is sent, the calling process must run with root privileges.
    /// </remarks>
    /// <param name="message">A tuple containing user name and group name is recognized. Other values are silently ignored.</param>
    public void Message(object message) {
        if (message is (string userName, string groupName)) {
            var user = UserInfo.FromName(userName);
            var group = GroupInfo.FromName(groupName);
            if (user is null || group is null) return;
            _ = new DPAPI_LinuxKey(user, group);
            CurrentContext = user.Uid;
        }
    }

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">Ignored.</param>
    /// <returns>Protected data.</returns>
    public byte[] Protect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => GetProtector(scope).Protect(data);

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">Ignored.</param>
    /// <returns>Decrypted data.</returns>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser)
        => GetProtector(scope).Unprotect(data);

    /// <summary>
    /// Gets the data protector for the specified user context.
    /// </summary>
    /// <param name="scope">Data protection scope.</param>
    /// <returns>Data protector instance.</returns>
    /// <exception cref="UnauthorizedAccessException">Insufficient privileges for the key access.</exception>
    private IDataProtector GetProtector(DataProtectionScope scope) {
        if (scope is DataProtectionScope.LocalMachine or DataProtectionScope.LocalSystem) return DPAPI_LinuxKey.LocalMachineScope.Protector;
        var currentUser = UserInfo.CurrentProcessUser;
        var contextUser = currentUser.Uid == CurrentContext ? currentUser : UserInfo.FromUid(CurrentContext)!;
        if (currentUser.Uid == CurrentContext) return DPAPI_LinuxKey.CurrentUserScope.Protector;
        if (DPAPI_LinuxKey.Available.TryGetValue(CurrentContext, out var key)) return key.Protector;
        try {
            return new DPAPI_LinuxKey(contextUser).Protector;
        }
        catch {
            throw new UnauthorizedAccessException("Insufficient privileges for the key access");
        }
    }

    /// <summary>
    /// Default user context.
    /// </summary>
    private int CurrentContext = UserInfo.CurrentProcessUser.Uid;

}