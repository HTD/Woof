namespace Woof.ServiceInstaller;

/// <summary>
/// Adds extension methods to the <see cref="ServiceMetadataSystemd"/> type.
/// Allows using the metadata to register, unregister and change the state of the system services.
/// </summary>
public static class ServiceMetadataSystemdTraits {

    /// <summary>
    /// Registers the system deamon.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the daemon is registered.</returns>
    public static ValueTask CreateAsync(this ServiceMetadataSystemd serviceMetadata)
        => new SystemDServiceInstaller(serviceMetadata).RegisterSystemDServiceAsync();

    /// <summary>
    /// Unregisters a service. Tries to stop it first.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="ValueTask"/> returning true if the command exited sucessfully.</returns>
    public static async ValueTask DeleteAsync(this ServiceMetadataSystemd serviceMetadata) {
        if (serviceMetadata is null) throw new ArgumentNullException(nameof(serviceMetadata));
        if (String.IsNullOrWhiteSpace(serviceMetadata.Name)) throw new InvalidOperationException(E.ServiceNameRequired);
        try { await StopAsync(serviceMetadata); } catch { }
        await new SystemDServiceInstaller(serviceMetadata).DeleteSystemDServiceAsync();
    }

    /// <summary>
    /// Installs a service described with <see cref="ServiceMetadataSystemd"/> on local machine.
    /// If the service is configured for automatic start it will be started.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <returns>Task completed when the service is installed.</returns>
    public static async Task InstallAsync(this ServiceMetadataSystemd service) {
        if (service.Name is null) throw new NullReferenceException();
        if (service.Start is null) throw new NullReferenceException();
        await service.CreateAsync();
        if (service.Start is StartType.Auto or StartType.DelayedAuto)
            await ServiceController.StartAsync(service.Name);
    }

    /// <summary>
    /// Starts the system daemon.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the service is started.</returns>
    public static ValueTask StartAsync(this ServiceMetadataSystemd serviceMetadata)
        => new ShellCommand($"systemctl start {serviceMetadata.Name}.service").ExecVoidAsync();

    /// <summary>
    /// Stops the system daeamon.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the service is stopped.</returns>
    public static ValueTask StopAsync(this ServiceMetadataSystemd serviceMetadata)
        => new ShellCommand($"systemctl stop {serviceMetadata.Name}.service").ExecVoidAsync();

    /// <summary>
    /// Creates host builder for the system daemon.
    /// </summary>
    /// <param name="serviceMetadata">Service metadata.</param>
    /// <returns><see cref="Task"/> completed when the shutdown is triggered.</returns>
    public static Task RunHostAsync<TService>(this ServiceMetadataSystemd serviceMetadata) where TService : class, IHostedService
        => Host.CreateDefaultBuilder()
        .ConfigureLogging(configureLogging => {
            configureLogging.AddFilter<EventLogLoggerProvider>(level => level >= serviceMetadata.EventLogLevelMinimal);
            configureLogging.SetMinimumLevel(serviceMetadata.EventLogLevelMinimal);
        })
        .ConfigureServices((hostContext, services) => services.AddHostedService<TService>())
        .UseSystemd().Build().RunAsync();

    /// <summary>
    /// Uninstall the service described with <see cref="ServiceMetadataSystemd"/> from local machine.
    /// Removes service's event log if applicable.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <returns>Task completed when the service is uninstalled.</returns>
    public static async Task UninstallAsync(this ServiceMetadataSystemd service) {
        if (service.Name is null) throw new NullReferenceException();
        await service.DeleteAsync();
    }

}