namespace Woof.ServiceInstaller;

/// <summary>
/// The implementing class contains service metadata for the Windows service.
/// </summary>
public class ServiceMetadataWindows : ServiceMetadata {

    /// <summary>
    /// Gets or sets the service description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the service's event log name.
    /// </summary>
    public string? EventLogName { get; set; }

    /// <summary>
    /// Gets or sets the service's event source name.
    /// </summary>
    public string? EventSourceName { get; set;  }

}