namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadata"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static partial class ServiceMetadataExtensions {

    /// <summary>
    /// Registers the service in the target OS.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is registered or an exception is thrown.</returns>
    public static ValueTask CreateAsync(this ServiceMetadata serviceMetadata)
        => serviceMetadata switch {
            null => throw new ArgumentNullException(nameof(serviceMetadata)),
            _ =>
                OS.IsWindows ? serviceMetadata.RegisterWindowsServiceAsync() :
                OS.IsLinux ? serviceMetadata.RegisterSystemDServiceAsync() :
                throw new PlatformNotSupportedException()
            };

    /// <summary>
    /// Starts a system service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is started or an exception is thrown.</returns>
    public static ValueTask StartAsync(this ServiceMetadata serviceMetadata)
        => serviceMetadata switch {
            null => throw new ArgumentNullException(nameof(serviceMetadata)),
            _ => string.IsNullOrWhiteSpace(serviceMetadata.Name)
                ? throw new InvalidOperationException(E.ServiceNameRequired)
                : StartAsync(serviceMetadata.Name)
        };

    /// <summary>
    /// Starts a system service specified by name.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    internal static async ValueTask StartAsync(string serviceName) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        if (String.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("Cannot be empty", nameof(serviceName));
        if (OS.IsWindows) await new ShellCommand($"sc start {serviceName}").ExecVoidAsync();
        else if (OS.IsLinux) await new ShellCommand($"systemctl start {serviceName}.service").ExecVoidAsync();
        else throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <param name="flag">Optional reason flag for the Windows OS.</param>
    /// <param name="reasonMajor">Reason major code.</param>
    /// <param name="reasonMinor">Reason minor code.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    public static ValueTask StopAsync(this ServiceMetadata serviceMetadata, ReasonFlag flag = ReasonFlag.Planned, ReasonMajor reasonMajor = ReasonMajor.Software, ReasonMinor reasonMinor = ReasonMinor.Installation)
        => serviceMetadata switch {
            null => throw new ArgumentNullException(nameof(serviceMetadata)),
            _ => string.IsNullOrWhiteSpace(serviceMetadata.Name)
            ? throw new InvalidOperationException(E.ServiceNameRequired)
            : StopAsync(serviceMetadata.Name, flag, reasonMajor, reasonMinor)
        };

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <param name="flag">Optional reason flag for the Windows OS.</param>
    /// <param name="reasonMajor">Reason major code.</param>
    /// <param name="reasonMinor">Reason minor code.</param>
    /// <returns><see cref="ValueTask"/> completed when the service is stopped or an exception is thrown.</returns>
    internal static async ValueTask StopAsync(string serviceName, ReasonFlag flag = ReasonFlag.Planned, ReasonMajor reasonMajor = ReasonMajor.Software, ReasonMinor reasonMinor = ReasonMinor.Installation) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        if (OS.IsWindows) {
            var reasonString = $"{(int)flag}:{(int)reasonMajor}:{(int)reasonMinor}";
            await new ShellCommand($"sc stop {serviceName} {reasonString}").ExecVoidAsync();
        }
        else if (OS.IsLinux) await new ShellCommand($"systemctl stop {serviceName}.service").ExecVoidAsync();
        else throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Creates host builder for the Windows Service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="Task"/> completed when the shutdown is triggered.</returns>
    public static Task RunHostAsync<TService>(this ServiceMetadata serviceMetadata) where TService : class, IHostedService
        => Host.CreateDefaultBuilder()
        .ConfigureLogging(configureLogging => {
            configureLogging.AddFilter<EventLogLoggerProvider>(level => level >= serviceMetadata.EventLogLevelMinimal);
            configureLogging.SetMinimumLevel(serviceMetadata.EventLogLevelMinimal);
        })
        .ConfigureServices((hostContext, services) => services
            .AddHostedService<TService>()
            .Configure<EventLogSettings>(config => {
                config.LogName = serviceMetadata.EventLogName;
                config.SourceName = serviceMetadata.EventSourceName;
            })).UseWindowsService().UseSystemd().Build().RunAsync();

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
                : IsRunningAsync(serviceMetadata.Name)
        };

    /// <summary>
    /// Tests if the service is running.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning true only when the service is installed and running.</returns>
    internal static async ValueTask<bool> IsRunningAsync(string serviceName) {
        if (serviceName is null) throw new ArgumentNullException(nameof(serviceName));
        var output = await QueryAsync(serviceName);
        return output is not null && output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);
    }

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
                : QueryAsync(serviceMetadata.Name)
        };

    /// <summary>
    /// Gets the service status if installed, null otherwise.
    /// </summary>
    /// <param name="serviceName">Service name.</param>
    /// <returns><see cref="ValueTask"/> returning status text returned by the target system.</returns>
    internal static async ValueTask<string?> QueryAsync(string serviceName) =>
        OS.IsWindows ? await new ShellCommand($"sc query {serviceName}").TryExecAsync() :
        OS.IsLinux ? await new ShellCommand($"service {serviceName} status").TryExecAsync() :
        throw new PlatformNotSupportedException();

    /// <summary>
    /// Unregisters a service. Tries to stop it first.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> returning true if the command exited sucessfully.</returns>
    public static async ValueTask DeleteAsync(this ServiceMetadata serviceMetadata) {
        if (serviceMetadata is null) throw new ArgumentNullException(nameof(serviceMetadata));
        if (String.IsNullOrWhiteSpace(serviceMetadata.Name)) throw new InvalidOperationException(E.ServiceNameRequired);
        try { await StopAsync(serviceMetadata); } catch { }
        if (OS.IsWindows) await new ShellCommand($"sc delete {serviceMetadata.Name}").ExecAndForgetAsync();
        else if (OS.IsLinux) await serviceMetadata.DeleteSystemDServiceAsync();
    }

}