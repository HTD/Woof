namespace Woof.ServiceInstaller;

/// <summary>
/// Major service stop reason code enumeation.
/// </summary>
public enum ReasonMajor {

    /// <summary>
    /// Not available.
    /// </summary>
    NA = 0,
    /// <summary>
    /// Other reason.
    /// </summary>
    Other = 1,
    /// <summary>
    /// Hardware event.
    /// </summary>
    Hardware = 2,
    /// <summary>
    /// Operating system request.
    /// </summary>
    OperatingSystem = 3,
    /// <summary>
    /// Software request.
    /// </summary>
    Software = 4,
    /// <summary>
    /// Application request.
    /// </summary>
    Application = 5

}
