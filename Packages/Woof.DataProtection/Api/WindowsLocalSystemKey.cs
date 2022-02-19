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
    private static DataProtectionKeyConfiguration GetConfiguration() {
        var systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var target = Path.Combine(systemDirectory, "DPAPI");
        var purpose = "Woof.DPAPI:Local System Service Key";
        return GetConfiguration(target, purpose);
    }

}