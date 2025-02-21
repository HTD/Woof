namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadataWindows"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static class ServiceMetadataWindowsTraits {

    /// <summary>
    /// Registers the Windows service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the service is registered.</returns>
    public static ValueTask CreateAsync(this ServiceMetadataWindows serviceMetadata)
        => new WindowsServiceInstaller(serviceMetadata).RegisterWindowsServiceAsync();

    /// <summary>
    /// Unregisters a service. Tries to stop it first.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> returning true if the command exited sucessfully.</returns>
    public static async ValueTask DeleteAsync(this ServiceMetadataWindows serviceMetadata) {
        ArgumentNullException.ThrowIfNull(serviceMetadata);
        if (String.IsNullOrWhiteSpace(serviceMetadata.Name)) throw new InvalidOperationException(E.ServiceNameRequired);
        try { await StopAsync(serviceMetadata); } catch { }
        await new ShellCommand($"sc delete {serviceMetadata.Name}").ExecAndForgetAsync();
    }

    /// <summary>
    /// Installs a service described with <see cref="ServiceMetadataWindows"/> on local machine.
    /// If the service is configured for automatic start it will be started.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <returns>Task completed when the service is installed.</returns>
    public static async Task InstallAsync(this ServiceMetadataWindows service) {
        await service.CreateAsync();
        if (service.Start is StartType.Auto or StartType.DelayedAuto)
            await ServiceController.StartAsync(service.Name);
    }

    /// <summary>
    /// Starts the windows service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the service is started.</returns>
    public static ValueTask StartAsync(this ServiceMetadataWindows serviceMetadata)
        => new ShellCommand($"sc start {serviceMetadata.Name}").ExecVoidAsync();

    /// <summary>
    /// Stops the windows service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <param name="flag">Optional reason flag.</param>
    /// <param name="reasonMajor">Reason major code.</param>
    /// <param name="reasonMinor">Reason minor code.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the service is stopped.</returns>
    public static ValueTask StopAsync(this ServiceMetadataWindows serviceMetadata, ReasonFlag flag = ReasonFlag.Planned, ReasonMajor reasonMajor = ReasonMajor.Software, ReasonMinor reasonMinor = ReasonMinor.Installation)
        => new ShellCommand($"sc stop {serviceMetadata.Name} {(int)flag}:{(int)reasonMajor}:{(int)reasonMinor}").ExecVoidAsync();

    /// <summary>
    /// Creates host builder for the Windows Service.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="Task"/> completed when the shutdown is triggered.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "This function is meant to use for Windows only")]
    public static Task RunHostAsync<TService>(this ServiceMetadataWindows serviceMetadata) where TService : class, IHostedService
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
            })
        )
        .UseWindowsService().Build().RunAsync();

    /// <summary>
    /// Uninstall the service described with <see cref="ServiceMetadataWindows"/> from local machine.
    /// Removes service's event log if applicable.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <param name="deleteEventLog">Set true to try to delete service's EventLog after the service is uninstalled.</param>
    /// <returns>Task completed when the service is uninstalled.</returns>
    public static async Task UninstallAsync(this ServiceMetadataWindows service, bool deleteEventLog = false) {
        if (service.Name is null) throw new NullReferenceException();
        await service.DeleteAsync();
        if (!deleteEventLog) return;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            try {
                if (service.EventSourceName != null) EventLog.DeleteEventSource(service.EventSourceName);
                if (service.EventLogName != null) EventLog.Delete(service.EventLogName);
            }
            catch { } // even if it fails we still unregistered the service, forget about EventLog.
        }
    }

}