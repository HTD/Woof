using Microsoft.Extensions.Configuration;

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
    /// <param name="settings">Configuration containing "Service" section or the "Service" section itself that have <see cref="Settings"/> properties set.</param>
    public static void Configure<TService>(ServiceMetadata settings) where TService : class, IHostedService {
        Settings = settings;
        var c = CommandLine.Default;
        c.Map<Options>();
        c.Delegates.Add(Options.Install, InstallAsync);
        c.Delegates.Add(Options.Uninstall, UninstallAsync);
        c.Delegates.Add(Options.DeleteLog, DeleteLog);
    }

    /// <summary>
    /// Runs the service host for the settings.
    /// </summary>
    /// <typeparam name="TService">A type of the class implementing the <see cref="IHostedService"/> interface.</typeparam>
    /// <returns>A <see cref="Task"/> completed when the Shutdown event is triggered.</returns>
    /// <exception cref="InvalidOperationException">Invalid settings.</exception>
    public static Task RunHostAsync<TService>() where TService : class, IHostedService
        => Settings is ServiceMetadataWindows wsSettings
            ? wsSettings.RunHostAsync<TService>() : Settings is ServiceMetadataSystemd sdSettings
            ? sdSettings.RunHostAsync<TService>() : throw new InvalidOperationException();

    /// <summary>
    /// Performs the service installation, displays the installation messages to the console.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the installer completes.</returns>
    private static async ValueTask InstallAsync() {
        if (Settings is null) throw new NullReferenceException();
        if (await Settings.IsRunningAsync()) {
            await UninstallAsync();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                do await Task.Delay(1000);
                while (await Settings.IsRunningAsync());
        }
        Console.WriteLine($"Installing {Settings.Name} service...");
        try {
            if (Settings is ServiceMetadataWindows windowsService)
                await windowsService.InstallAsync();
            else if (Settings is ServiceMetadataSystemd systemDaemon)
                await systemDaemon.InstallAsync();
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
        if (Settings is null) throw new NullReferenceException();
        Console.WriteLine($"Uninstalling {Settings.Name} service...");
        try {
            if (Settings is ServiceMetadataWindows wsSettings)
                await wsSettings.UninstallAsync();
            else if (Settings is ServiceMetadataSystemd sdSettings)
                await sdSettings.UninstallAsync();
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
        if (Settings is not ServiceMetadataWindows wsSettings || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try {
            Console.WriteLine($"Deleting log ({wsSettings.EventLogName})...");
            EventLog.Delete(wsSettings.EventLogName);
            Console.WriteLine($"Deleting event source ({wsSettings.EventSourceName})...");
            EventLog.DeleteEventSource(wsSettings.EventSourceName);
        }
        catch { }
        Console.WriteLine(Done);
    }

    /// <summary>
    /// Gets or sets the current service settings.
    /// </summary>
    public static ServiceMetadata? Settings { get; set; }

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