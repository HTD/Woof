namespace Woof.ServiceInstaller;

/// <summary>
/// Provides basic methods for controlling the system services by name.
/// </summary>
public static class ServiceController {

    /// <summary>
    /// Starts a system service specified by name.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    public static ValueTask StartAsync(string serviceName)
        => ServiceMetadataExtensions.StartAsync(serviceName);

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="flag">Optional reason flag for the Windows OS.</param>
    /// <param name="reasonMajor">Reason major code.</param>
    /// <param name="reasonMinor">Reason minor code.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    public static ValueTask StopAsync(string serviceName, ReasonFlag flag = ReasonFlag.Planned, ReasonMajor reasonMajor = ReasonMajor.Software, ReasonMinor reasonMinor = ReasonMinor.Installation)
        => ServiceMetadataExtensions.StopAsync(serviceName, flag, reasonMajor, reasonMinor);

    /// <summary>
    /// Tests if the service is running.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning true only when the service is installed and running.</returns>
    public static ValueTask<bool> IsRunningAsync(string serviceName) => ServiceMetadataExtensions.IsRunningAsync(serviceName);

    /// <summary>
    /// Gets the service status if installed, null otherwise.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning status text returned by the target system.</returns>
    public static ValueTask<string?> QueryAsync(string serviceName) => ServiceMetadataExtensions.QueryAsync(serviceName);

}
