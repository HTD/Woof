namespace Woof.ServiceInstaller;

/// <summary>
/// The implementing class contains service metadata with Name, DisplayName, Start and EventLogLevelMinimal properties.
/// </summary>
#pragma warning disable 8618
public abstract class ServiceMetadata {

    /// <summary>
    /// Gets or sets the system strong name for the service.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name for the service.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the service's start type.
    /// Should be one of the <see cref="StartType"/> values.
    /// </summary>
    public string? Start { get; set; } = StartType.Auto;

    /// <summary>
    /// Gets or sets the minimal logging level for the class.
    /// </summary>
    public LogLevel EventLogLevelMinimal { get; set; } = LogLevel.Debug;

}
#pragma warning restore 8618