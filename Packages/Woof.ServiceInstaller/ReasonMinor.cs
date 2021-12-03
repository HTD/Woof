namespace Woof.ServiceInstaller;

/// <summary>
/// Minor service stop reason code enumeation.
/// </summary>
public enum ReasonMinor {

    /// <summary>
    /// Not available.
    /// </summary>
    NA = 0,
    /// <summary>
    /// Other reason.
    /// </summary>
    Other = 1,
    /// <summary>
    /// System maintenance.
    /// </summary>
    Maintenance = 2,
    /// <summary>
    /// System installation.
    /// </summary>
    Installation = 3,
    /// <summary>
    /// Service upgrade.
    /// </summary>
    Upgrade = 4,
    /// <summary>
    /// Service reconfiguration.
    /// </summary>
    Reconfiguration = 5,
    /// <summary>
    /// Service stopped responding.
    /// </summary>
    Hung = 6,
    /// <summary>
    /// Service is unstable.
    /// </summary>
    Unstable = 7,
    /// <summary>
    /// Disk event.
    /// </summary>
    Disk = 8,
    /// <summary>
    /// Network event.
    /// </summary>
    NetworkCard = 9,
    /// <summary>
    /// Environment request.
    /// </summary>
    Environment = 10,
    /// <summary>
    /// Driver request.
    /// </summary>
    HardwareDriver = 11,
    /// <summary>
    /// Other driver request.
    /// </summary>
    OtherDriver = 12,
    /// <summary>
    /// Service pack request.
    /// </summary>
    ServicePack = 13,
    /// <summary>
    /// Software update.
    /// </summary>
    SoftwareUpdate = 14,
    /// <summary>
    /// Security fix.
    /// </summary>
    SecurityFix = 15,
    /// <summary>
    /// Security related.
    /// </summary>
    Security = 16,
    /// <summary>
    /// Network connectivity related.
    /// </summary>
    NetworkConnectivity = 17,
    /// <summary>
    /// WMI related.
    /// </summary>
    WMI = 18,
    /// <summary>
    /// Service pack uninstall.
    /// </summary>
    ServicePackUninstall = 19,
    /// <summary>
    /// Software Update Uninstall
    /// </summary>
    SoftwareUpdateUninstall = 20,
    /// <summary>
    /// Security Fix Uninstall
    /// </summary>
    SecurityFixUninstall = 22,
    /// <summary>
    /// MMC related.
    /// </summary>
    MMC = 23,

}
