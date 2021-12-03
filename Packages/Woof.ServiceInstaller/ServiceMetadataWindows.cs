namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadata"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static partial class ServiceMetadataExtensions {

    /// <summary>
    /// Registers a Windows Service in Windows OS.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the service is registered or an exception is thrown.</returns>
    internal static ValueTask RegisterWindowsServiceAsync(this ServiceMetadata serviceMetadata)
        => new WindowsServiceInstaller(serviceMetadata).RegisterWindowsServiceAsync();

}