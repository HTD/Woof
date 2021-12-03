namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadata"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static partial class ServiceMetadataExtensions {

    /// <summary>
    /// Registers a systemd service in Linux OS.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is registered or an exception is thrown.</returns>
    internal static ValueTask RegisterSystemDServiceAsync(this ServiceMetadata serviceMetadata)
        => new SystemDServiceInstaller(serviceMetadata).RegisterSystemDServiceAsync();

    /// <summary>
    /// Deletes a systemd service from Linux OS.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is deleted.</returns>
    internal static ValueTask DeleteSystemDServiceAsync(this ServiceMetadata serviceMetadata)
        => new SystemDServiceInstaller(serviceMetadata).DeleteSystemDServiceAsync();

}