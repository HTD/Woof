await Settings.Default.LoadAsync();
ServiceMetadata serviceSettings = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? Settings.Default.WindowsService : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
    ? Settings.Default.SystemDaemon : throw new PlatformNotSupportedException();

var commandLine = CommandLine.Default;
ServiceInstaller.Configure<TestService.TestService>(serviceSettings);
if (args?.Length > 0) {
    commandLine.Map<Options>();
    commandLine.Delegates.Add(Options.Help, Help);
    commandLine.Delegates.Add(Options.Query, QueryAsync);
    commandLine.Delegates.Add(Options.Running, IsRunningAsync);
    commandLine.Parse(args);
    var errors = CommandLine.ValidationErrors;
    if (errors is not null) Error(errors);
    else await commandLine.RunDelegatesAsync();
}
else await ServiceInstaller.RunHostAsync<TestService.TestService>();

static void Help() => Console.WriteLine(CommandLine.Help);

static void Error(string? errorText) {
    Console.WriteLine(errorText);
    Console.WriteLine(CommandLine.Usage);
}

static async ValueTask IsRunningAsync()
    => Console.WriteLine((await ServiceInstaller.Settings!.IsRunningAsync()) ? "RUNNING" : "NOT RUNNING");

static async ValueTask QueryAsync()
    => Console.WriteLine(await ServiceInstaller.Settings!.QueryAsync());

[Usage("(sudo) {command} [--install|--uninstall|--help]")]
enum Options {
    [Option("q|query", null, "Shows current service status.")]
    Query,
    [Option("r|running", null, "Tests if the service is running.")]
    Running,
    [Option("?|h|help", null, "Displays this help message.")]
    Help
}