namespace Woof.ServiceInstaller;

/// <summary>
/// Reason flags for stopping the service.
/// </summary>
[Flags]
public enum ReasonFlag {

    /// <summary>
    /// Not available.
    /// </summary>
    NA = 0,
    /// <summary>
    /// Unplanned.
    /// </summary>
    Unplanned = 1,
    /// <summary>
    /// Custom.
    /// </summary>
    Custom = 2,
    /// <summary>
    /// Planned.
    /// </summary>
    Planned = 4

}