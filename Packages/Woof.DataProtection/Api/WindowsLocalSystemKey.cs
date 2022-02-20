namespace Woof.DataProtection.Api;

/// <summary>
/// Windows Local System data protection key for services.
/// </summary>
/// <remarks>
/// This will fail miserably when used with non-administrative account for the first time.
/// </remarks>
public class WindowsLocalSystemKey : DataProtectionKeyBase {

    /// <summary>
    /// Configures Windows Local System data protection key for services.
    /// </summary>
    public WindowsLocalSystemKey() : base(GetConfiguration()) { }

    /// <summary>
    /// Gets the configuration for the base constructor.
    /// </summary>
    /// <returns>Data protection key configuration.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Used only for Windows")]
    private static DataProtectionKeyConfiguration GetConfiguration() {
        var systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var target = Path.Combine(systemDirectory, "DPAPI");
        var targetDirectory = new DirectoryInfo(target);
        if (!targetDirectory.Exists) {
            targetDirectory.Create();
            targetDirectory = new DirectoryInfo(target);
            var targetSecurity = new DirectorySecurity();
            var acl = targetSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
            targetSecurity.SetAccessRuleProtection(true, false);
            foreach (FileSystemAccessRule ace in acl) targetSecurity.PurgeAccessRules(ace.IdentityReference);
            targetSecurity.AddAccessRule(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
            targetSecurity.AddAccessRule(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
            targetDirectory.SetAccessControl(targetSecurity);
        }
        var purpose = "Woof.DPAPI:Local System Service Key";
        return GetConfiguration(target, purpose);
    }

}