using System;
using System.Threading.Tasks;

using Woof;
using Woof.Config.Protected;
using Woof.ServiceInstaller;

var config = new JsonConfigProtected().Protect();
var commandLine = CommandLine.Default;

ServiceInstaller.Configure<TestService.TestService>(config);
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
else await ServiceInstaller.ServiceMetadata!.RunHostAsync<TestService.TestService>();

static void Help() => Console.WriteLine(CommandLine.Help);

static void Error(string? errorText) {
    Console.WriteLine(errorText);
    Console.WriteLine(CommandLine.Usage);
}

static async ValueTask IsRunningAsync()
    => Console.WriteLine((await ServiceInstaller.ServiceMetadata!.IsRunningAsync()) ? "RUNNING" : "NOT RUNNING");

static async ValueTask QueryAsync()
    => Console.WriteLine(await ServiceInstaller.ServiceMetadata!.QueryAsync());

[Usage("(sudo) {command} [--install|--uninstall|--help]")]
enum Options {
    [Option("q|query", null, "Shows current service status.")]
    Query,
    [Option("r|running", null, "Tests if the service is running.")]
    Running,
    [Option("?|h|help", null, "Displays this help message.")]
    Help
}