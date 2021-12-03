# Woof.ServiceInstaller

A part of the [**Woof Tookit**](../../Readme.md)
by **[CodeDog](https://www.codedog.pl)**.

Distributed under [MIT License](https://en.wikipedia.org/wiki/MIT_License).
(c)2021 by CodeDog, All rights reserved.

---

## About

Installs and runs system services on Linux and Windows.

When `{command}` is your compiled service as one file native executable:

- For help use `{command} --help`.
- To install: `sudo {command} --install`
- To uninstall: `sudo {command} --uninstall`
- To delete Windows Event Log: `{command} --delete-log`

### Windows

Install your executable with a [Microsoft Installer](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2022InstallerProjects) first.
For protected configurations use `DataProtectionScope.LocalMachine`.
The service will be registered in the location it was run with the `--install` option.

On Windows, Windows Installer (MSI) is the default way to install applications,
so the service installer does not copy the files elsewhere.

To check if the service is running use either `Services` panel,
or type `sc query {serviceName}`  in `cmd`.

You can also use Windows Event Log to look for entries from the service.

The service runs in `LocalSystem` user context.

When the service is uninstalled with `--uninstal` option it will be automatically stopped,
then unregistered. The logs will stay until deleted with `--delete-log` option.

### Linux

When run with sudo and `--install` option, the service will be copied
to the `/srv/{serviceName}` directory and the permission for the directory
will be set to user `service` (the user account set in configuration).

If the system user `service` does not exist it will be created.
The service will be run in `service` user context.
The service can be configured to use any system user and group.
Both user and group will be created if they don't exist.

(See the [ServiceMetadata](https://github.com/HTD/Woof.ServiceInstaller/blob/master/Woof.ServiceInstaller/ServiceMetadata.cs) XML documentation.)

Linux system services are meant to be installed from a console as root user.

To check if the service is running type `service {serviceName} status`.

The logging events from the service will go to the main system log.

When the service is uninstalled with `--uninstall` option
the service installed files will be deleted but the `service` user will stay.

Also the Linux logs are not cleared.

### Common

For service configuration options see the configuration `JSON` in example,
also check the [ServiceMetadata](https://github.com/HTD/Woof.ServiceInstaller/blob/master/Woof.ServiceInstaller/ServiceMetadata.cs) class XML documentation.

The configuration for the example service is provided with
[Woof.Config](https://github.com/HTD/Woof.Config) package.

Installing the service with the same name as an existing one will cause the
current one to be automatically stopped and uninstalled first.

Naturally, the service will be uninstalled if it was installed with
`Woof.ServiceInstaller` and its configuration matches.

The described feature is designed for easy software upgrades.
Just install the new version, the service will be upgraded,
no extra steps needed.

The installed services can be controlled (started, stopped, queried and unregistered)
using the target system service controller.

The default configuration makes the service start as soon as it's installed.

The `Woof.ServiceInstaller` package was made to install self-hosting `WebSocket` APIs on both
Linux and Windows hosts, however additional networking setup is needed for that.

To build a [WebSocket](https://en.wikipedia.org/wiki/WebSocket) API use [Woof.Net.WebSockets](https://github.com/HTD/Woof.Net.WebSockets) package.

### Operation / Design

Both [Windows Service](https://docs.microsoft.com/en-us/dotnet/core/extensions/windows-service) and [Linux systemd](https://wiki.archlinux.org/title/systemd) use the [IHostedService](https://www.google.com/search?q=IHostedService&oq=IHostedService&aqs=edge..69i57j0i512l5j0i10i512j0i512.3496j0j1&sourceid=chrome&ie=UTF-8) interface.

It allows the service to be started and stopped.

The service executable is a normal executable that starts the service if run
without parameters. The process never exists unless it receives the stop signal
from the host. The services should use the `CancellationToken` provided with
`Start()` and `Stop()` methods.

The services will restart after the host is restarted.

### The command line interface

The service installer uses the [Woof.CommandLine](https://github.com/HTD/Woof.CommandLine) to provide
a unified, POSIX type command line interface.

By default the service installer automatically binds the
`--install|-i`, `--uninstall|-u` and `--delete-log|-d` options
to its internal handlers.

It doesn't bind the `--help|-h|-?` option, it should be done in
user code, because the user code can provide its own additional
options. `--query|-q` and `--running|r` options are defined in the
example to illustrate how it's done.

The automatically generated help for the options can be obtained
from `CommandLine.Help` property.

Similarily, command line validation errors can be obtained from
`CommandLine.ValidationErrors` property after the `Parse()`
method was called.

## Usage

Here's example `Program.cs`:
```cs
using System;
using System.Threading.Tasks;

using Woof;
using Woof.Config;
using Woof.ServiceInstaller;

var config = new JsonConfig();
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
```

To execute code before or after installation / uninstallation simply check
if the install/uninstall options are present before and/or after [RunDelegatesAsync](https://github.com/HTD/Woof.CommandLine/blob/13de430e26851419e1ac9c20141dc9f8341f0e8f/Woof.CommandLine/API/CommandLineParser.cs#L202)
like this:
```cs
if (commandLine.HasOption(ServiceInstaller.Options.Install)) {
    // code to execute
}
```

The installed service is started with [RunHostAsync](https://github.com/HTD/Woof.ServiceInstaller/blob/78a7e28fd2c462ab3d339b9357b304259bf3ad3d/Woof.ServiceInstaller/ServiceMetadataExtensions.cs#L97):
```cs
ServiceInstaller.ServiceMetadata!.RunHostAsync<TestService.TestService>();
```

If executing of some external commands is needed before or after the installation,
[Shell.ExecAsync](https://github.com/HTD/Woof.LinuxAdmin/blob/2cad3b3e7e8f18009ec6026d040b1df007cc7763/Woof.LinuxAdmin/Shell.cs#L22) method from [Woof.LinuxAdmin](https://github.com/HTD/Woof.LinuxAdmin/) dependency can be used for that.
If the command executed that way fails, the method will throw an `InvalidOperationException`
with the error message obtained from the command output.

For more information see the included example project.

---

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.