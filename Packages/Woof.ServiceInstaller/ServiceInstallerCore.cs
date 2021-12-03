using System.Diagnostics;

namespace Woof.ServiceInstaller;

/// <summary>
/// Allows registering and unregistering services on Windows systems.
/// </summary>
public class ServiceInstallerCore {

    /// <summary>
    /// Installs a service described with <see cref="ServiceMetadata"/> on local machine.
    /// If the service is configured for automatic start it will be started.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <returns>Task completed when the service is installed.</returns>
    public static async Task InstallAsync(ServiceMetadata service) {
        if (service.Name is null) throw new NullReferenceException();
        if (service.Start is null) throw new NullReferenceException();
        await service.CreateAsync();
        if (service.Start is StartType.Auto or StartType.DelayedAuto)
            await ServiceController.StartAsync(service.Name);
    }

    /// <summary>
    /// Uninstall the service described with <see cref="ServiceMetadata"/> from local machine.
    /// Removes service's event log if applicable.
    /// </summary>
    /// <param name="service">Service metadata.</param>
    /// <param name="deleteEventLog">Set true to try to delete service's EventLog after the service is uninstalled.</param>
    /// <returns>Task completed when the service is uninstalled.</returns>
    public static async Task UninstallAsync(ServiceMetadata service, bool deleteEventLog = false) {
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
