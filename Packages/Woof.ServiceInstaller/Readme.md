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
For protected configurations use `DataProtectionScope.LocalSystem`.
The service will be registered in the location it was run with the `--install` option.

On Windows, Windows Installer (MSI) is the default way to install applications,
so the service installer does not copy the files elsewhere.

To check if the service is running use either `Services` panel,
or type `sc query {serviceName}`  in `cmd`.

You can also use Windows Event Log to look for entries from the service.

The service runs in `LocalSystem` user context.
The account can be changed to `NetworkService` via the `Account` property
of the service configuration metadata. However - the data protection feature
will work ONLY with `LocalSystem`. This applies to `Woof.Settings.Protected` and
`Woof.Settings.AKV` packages.

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

### Usage

1. Create a console project.
2. Add `Woof.Settings` / `Woof.Settings.AKV` / `Woof.Settings.Protected` package reference.
3. Optionally, if data protection is used and the service supports Linux systems,
   add `Woof.DataProtection.Linux` package reference.
4. Create a service configuration JSON file like:
   ```json
   {
       "windowsService": {
           "name": "testsvc",
           "displayName": "Woof Service Installer Test",
           "description": "Tests the Woof.ServiceInstaller.",
           "eventLogName": "Woof.ServiceInstaller",
           "eventSourceName": "Woof.ServiceInstaller.Test",
           "eventLogLevelMinimal": "debug"
       },
       "systemDaemon": {
           "name": "testsvc",
           "displayName": "Woof Service Installer Test",
           "eventLogLevelMinimal": "debug",
           "user": "service",
           "group": "service"
       }
   }
   ```
5. Create settings class like:
   ```cs
   public class Settings : JsonSettingsAkv<Settings> {

       private Settings() : base(DataProtectionScope.LocalSystem) { }
       public static Settings Default { get; } = new Settings();
       public ServiceMetadataWindows WindowsService { get; } = new();
       public ServiceMetadataSystemd SystemDaemon { get; } = new();

   }
   ```
5. Save the file with the name matching the assembly name with `.json` extension.
6. Set the `Copy to Output Directory` property to `Copy if newer`.
7. Optionally create a corresponding `.access.json` file as described in
   `Woof.Settings.AKV` documentation.
8. Create `Program.cs` like:
   ```cs
    ServiceInstaller.AssertAdmin(args);
    await Settings.Default.LoadAsync();
    ServiceMetadata serviceSettings = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Settings.Default.WindowsService : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? Settings.Default.SystemDaemon : throw new PlatformNotSupportedException();
    await ServiceInstaller.ConfigureAndRunAsync<TestService>(serviceSettings, args);
   ```

The service can be tested, installed and uninstalled with admin access only.
Run without parameters to test the service operation.
Run with `-?` switch to display installer help.

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

## Disclaimer

Woof Toolkit is a work in progress in constant development,
however it's carefully maintained with production code quality.

**PLEASE report all issues on GitHub!**

Describe how to reproduce an issue.
Also feel free to suggest new features or improvements.