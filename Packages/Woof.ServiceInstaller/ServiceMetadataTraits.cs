namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadata"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static class ServiceMetadataTraits {

    /// <summary>
    /// Tests if the service is running.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> returning true only when the service is installed and running.</returns>
    public static ValueTask<bool> IsRunningAsync(this ServiceMetadata serviceMetadata)
        => serviceMetadata switch {
            null => throw new ArgumentNullException(nameof(serviceMetadata)),
            _ => string.IsNullOrWhiteSpace(serviceMetadata.Name)
                ? throw new InvalidOperationException(E.ServiceNameRequired)
                : ServiceController.IsRunningAsync(serviceMetadata.Name)
        };

    /// <summary>
    /// Gets the service status if installed, null otherwise.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> returning status text returned by the target system.</returns>
    public static ValueTask<string?> QueryAsync(this ServiceMetadata serviceMetadata)
        => serviceMetadata switch {
            null => throw new ArgumentNullException(nameof(serviceMetadata)),
            _ => string.IsNullOrWhiteSpace(serviceMetadata.Name)
                ? throw new InvalidOperationException(E.ServiceNameRequired)
                : ServiceController.QueryAsync(serviceMetadata.Name)
        };

}