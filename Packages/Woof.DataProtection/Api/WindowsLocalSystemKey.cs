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
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Used only for Windows")]
    private static DataProtectionKeyConfiguration GetConfiguration() {
        var systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var target = Path.Combine(systemDirectory, "DPAPI");
        var targetDirectory = new DirectoryInfo(target);
        if (!targetDirectory.Exists) {
            targetDirectory.Create();
            targetDirectory = new DirectoryInfo(target);
            DirectorySecurity ds = new();
            var rules = ds.GetAccessRules(includeExplicit: true, includeInherited: true, typeof(NTAccount));
            ds.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            foreach (FileSystemAccessRule rule in rules) ds.PurgeAccessRules(rule.IdentityReference);
            ds.AddAccessRule(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
            ds.AddAccessRule(new FileSystemAccessRule("CREATOR OWNER", FileSystemRights.FullControl, AccessControlType.Allow));
            targetDirectory.SetAccessControl(ds);
        }
        var purpose = "Woof.DPAPI:Local System Service Key";
        return GetConfiguration(target, purpose);
    }

}