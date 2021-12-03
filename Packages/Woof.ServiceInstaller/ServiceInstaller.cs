using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace Woof.ServiceInstaller;

/// <summary>
/// Service installer command line interface.
/// </summary>
public static class ServiceInstaller {

    const string Done = "Done.";

    /// <summary>
    /// Configures <see cref="Options.Install"/> and <see cref="Options.Uninstall"/> command line options.<br/>
    /// Call <see cref="CommandLineParser.RunDelegatesAsync"/> to handle installer options.<br/>
    /// Use <see cref="CommandLine.Help"/> or <see cref="CommandLine.OptionsSummary"/> to get automatic documentation of the options.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="config">Configuration containing "Service" section or the "Service" section itself that have <see cref="ServiceMetadata"/> properties set.</param>
    public static void Configure<TService>(IConfiguration config) where TService : class, IHostedService {
        ServiceMetadata = ServiceMetadata.Load<TService>(config);
        var c = CommandLine.Default;
        c.Map<Options>();
        c.Delegates.Add(Options.Install, InstallAsync);
        c.Delegates.Add(Options.Uninstall, UninstallAsync);
        c.Delegates.Add(Options.DeleteLog, DeleteLog);
    }

    /// <summary>
    /// Performs the service installation, displays the installation messages to the console.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the installer completes.</returns>
    private static async ValueTask InstallAsync() {
        if (ServiceMetadata is null) throw new NullReferenceException();
        if (await ServiceMetadata.IsRunningAsync()) {
            await UninstallAsync();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                do await Task.Delay(1000);
                while (await ServiceMetadata.IsRunningAsync());
        }
        Console.WriteLine($"Installing {ServiceMetadata.Name} service...");
        try {
            await ServiceInstallerCore.InstallAsync(ServiceMetadata);
            Console.WriteLine(Done);
        }
        catch (Exception installerException) {
            Console.WriteLine($"Installer failed with exception: {installerException.Message}");
            Console.WriteLine($"Stack trace:" + Environment.NewLine + installerException.StackTrace);
        }
    }

    /// <summary>
    /// Performs the service uninstallation, displays the installation messages to the console.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the installer completes.</returns>
    private static async ValueTask UninstallAsync() {
        if (ServiceMetadata is null) throw new NullReferenceException();
        Console.WriteLine($"Uninstalling {ServiceMetadata.Name} service...");
        try {
            await ServiceInstallerCore.UninstallAsync(ServiceMetadata);
            Console.WriteLine(Done);
        }
        catch (Exception installerException) {
            Console.WriteLine($"Installer failed with exception: {installerException.Message}");
            Console.WriteLine($"Stack trace:" + Environment.NewLine + installerException.StackTrace);
        }
    }

    /// <summary>
    /// Deletes the service log.
    /// </summary>
    private static void DeleteLog() {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        if (ServiceMetadata is null) throw new NullReferenceException();
        try {
            Console.WriteLine($"Deleting log ({ServiceMetadata.EventLogName})...");
            EventLog.Delete(ServiceMetadata.EventLogName);
            Console.WriteLine($"Deleting event source ({ServiceMetadata.EventSourceName})...");
            EventLog.DeleteEventSource(ServiceMetadata.EventSourceName);
        }
        catch { }
        Console.WriteLine(Done);
    }

    /// <summary>
    /// Stores data of the currenc service configuration. Call <see cref="Configure{TService}(IConfiguration)"/> to set.
    /// </summary>
    public static ServiceMetadata? ServiceMetadata { get; set; }

    /// <summary>
    /// Installation options.
    /// </summary>
    [Usage("{command} <--install|--uninstall>")]
    public enum Options {

        /// <summary>
        /// Install option.
        /// </summary>
        [Option("i|install", null, "Installs the service in the system.")]
        Install,

        /// <summary>
        /// Uninstall option.
        /// </summary>
        [Option("u|uninstall", null, "Uninstalls the service from the system.")]
        Uninstall,

        /// <summary>
        /// Deletes the log for the service.
        /// </summary>
        [Option("d|delete-log", null, "Deletes the service log.", OSPlatform = "Windows")]
        DeleteLog

    }

}
