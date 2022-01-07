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
    public static async ValueTask StartAsync(string serviceName) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("Cannot be empty", nameof(serviceName));
        if (OS.IsWindows) await new ShellCommand($"sc start {serviceName}").ExecVoidAsync();
        else if (OS.IsLinux) await new ShellCommand($"systemctl start {serviceName}.service").ExecVoidAsync();
        else throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="flag">Optional reason flag for the Windows OS.</param>
    /// <param name="reasonMajor">Reason major code.</param>
    /// <param name="reasonMinor">Reason minor code.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    public static async ValueTask StopAsync(string serviceName, ReasonFlag flag = ReasonFlag.Planned, ReasonMajor reasonMajor = ReasonMajor.Software, ReasonMinor reasonMinor = ReasonMinor.Installation) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        if (OS.IsWindows) {
            var reasonString = $"{(int)flag}:{(int)reasonMajor}:{(int)reasonMinor}";
            await new ShellCommand($"sc stop {serviceName} {reasonString}").ExecVoidAsync();
        }
        else if (OS.IsLinux) await new ShellCommand($"systemctl stop {serviceName}.service").ExecVoidAsync();
        else throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Tests if the service is running.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning true only when the service is installed and running.</returns>
    public static async ValueTask<bool> IsRunningAsync(string serviceName) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        var output = await QueryAsync(serviceName);
        return output is not null && output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the service status if installed, null otherwise.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning status text returned by the target system.</returns>
    public static async ValueTask<string?> QueryAsync(string serviceName) =>
        OS.IsWindows ? await new ShellCommand($"sc query {serviceName}").TryExecAsync() :
        OS.IsLinux ? await new ShellCommand($"service {serviceName} status").TryExecAsync() :
        throw new PlatformNotSupportedException();

}
