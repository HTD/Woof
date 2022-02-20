namespace Woof.ServiceInstaller;

/// <summary>
/// Service installer command line interface.
/// </summary>
public static class ServiceInstaller {

    const string Done = "Done.";

    /// <summary>
    /// Asserts system administrator access to the service command line interface.
    /// </summary>
    /// <remarks>
    /// Run BEFORE loading the protected configuration!
    /// </remarks>
    /// <param name="args">Command line arguments.</param>
    public static void AssertAdmin(string[] args) {
        if (args.Length > 0 && !CurrentUser.IsAdmin) {
            Console.WriteLine("The system service access requires admin access.");
            Environment.Exit(-1);
        }
    }

    /// <summary>
    /// Configures <see cref="Options.Install"/> and <see cref="Options.Uninstall"/> command line options.<br/>
    /// Call <see cref="CommandLineParser.RunDelegatesAsync"/> to handle installer options.<br/>
    /// Use <see cref="CommandLine.Help"/> or <see cref="CommandLine.OptionsSummary"/> to get automatic documentation of the options.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="settings">Service metadata.</param>
    public static void Configure<TService>(ServiceMetadata settings) where TService : class, IHostedService {
        Settings = settings;
        var c = CommandLine.Default;
        c.Map<Options>();
        c.Delegates.Add(Options.Help, Help);
        c.Delegates.Add(Options.Query, QueryAsync);
        c.Delegates.Add(Options.Install, InstallAsync);
        c.Delegates.Add(Options.Uninstall, UninstallAsync);
        c.Delegates.Add(Options.DeleteLog, DeleteLog);
    }

    /// <summary>
    /// Configures and runs either the command line interface or the service host.
    /// </summary>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <param name="settings">Service metadata.</param>
    /// <param name="args">Command line arguments for the program.</param>
    /// <returns>A <see cref="ValueTask"/> completed when the program completes.</returns>
    public static async ValueTask ConfigureAndRunAsync<TService>(ServiceMetadata settings, string[] args) where TService : class, IHostedService {
        Configure<TService>(settings);
        if (args.Length > 0) {
            var c = CommandLine.Default;
            c.Parse(args);
            var errors = CommandLine.ValidationErrors;
            if (errors is not null) Error(errors);
            else await c.RunDelegatesAsync();
        }
        else await RunHostAsync<TService>();
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
    /// Displays the automatic help for the installer.
    /// </summary>
    static void Help() => Console.WriteLine(CommandLine.Help);

    /// <summary>
    /// Displays the errors and usage in case of invalid command line syntax.
    /// </summary>
    /// <param name="errorText"></param>
    static void Error(string? errorText) {
        Console.WriteLine(errorText);
        Console.WriteLine(CommandLine.Usage);
    }

    /// <summary>
    /// Queries the operation system for the service status.
    /// </summary>
    /// <returns></returns>
    static async ValueTask QueryAsync() => Console.WriteLine(await ServiceInstaller.Settings!.QueryAsync());

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
    [Usage("{command} <--install|--uninstall|--query|--help>")]
    public enum Options {

        /// <summary>
        /// Help option.
        /// </summary>
        [Option("?|h|help", null, "Displays this help message.")]
        Help,

        /// <summary>
        /// Query service option.
        /// </summary>
        [Option("q|query", null, "Shows current service status.")]
        Query,

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