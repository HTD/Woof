namespace Woof.ServiceInstaller;

/// <summary>
/// The implementing class contains service metadata for the system daemon.
/// </summary>
public sealed class ServiceMetadataSystemd : ServiceMetadata {

    /// <summary>
    /// Gets or sets a user account name that will be used to run the service.
    /// </summary>
    public string? User { get; set; } = "service";

    /// <summary>
    /// Gets or sets a user group that will be used to run the service.
    /// </summary>
    public string? Group { get; set; } = "service";

}