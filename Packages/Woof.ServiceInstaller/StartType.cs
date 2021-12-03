namespace Woof.ServiceInstaller;

/// <summary>
/// Service start type values.
/// </summary>
public static class StartType {

    /// <summary>
    /// A device driver loaded by the boot loader.
    /// </summary>
    public const string Boot = "boot";
    /// <summary>
    /// A service started during kernel initialization.
    /// </summary>
    public const string System = "system";
    /// <summary>
    /// A service automatically started at boot time, even if no user logs on.
    /// </summary>
    public const string Auto = "auto";
    /// <summary>
    /// A service that must be manually started (the default).
    /// </summary>
    public const string Demand = "demand";
    /// <summary>
    /// A service that can't be started.
    /// </summary>
    public const string Disabled = "disabled";
    /// <summary>
    /// A service that starts automatically a short time after other auto services are started. 
    /// </summary>
    public const string DelayedAuto = "delayed-auto";

}
