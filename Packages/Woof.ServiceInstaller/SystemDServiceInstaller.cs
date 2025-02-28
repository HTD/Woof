﻿namespace Woof.ServiceInstaller;

/// <summary>
/// A tool to install .NET services as Linux systemd.
/// </summary>
internal class SystemDServiceInstaller {

    /// <summary>
    /// Initializes the instance.
    /// </summary>
    /// <param name="serviceMetadata">Service medatada.</param>
    /// <exception cref="ArgumentException">Metadata Name property is null.</exception>
    /// <exception cref="ArgumentNullException">Argument is null.</exception>
    public SystemDServiceInstaller(ServiceMetadataSystemd serviceMetadata) {
        if (!OS.IsLinux) throw new PlatformNotSupportedException();
        ArgumentNullException.ThrowIfNull(serviceMetadata);
        if (serviceMetadata.Name is null || serviceMetadata.Name.Length < 1)
            throw new ArgumentException(E.ServiceNameRequired, nameof(serviceMetadata));
        Metadata = serviceMetadata;
    }

    /// <summary>
    /// Registers a systemd service in Linux OS.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the service is registered or an exception is thrown.</returns>
    /// <exception cref="InvalidOperationException">LinuxName property not set in metadata.</exception>
    public async ValueTask RegisterSystemDServiceAsync() {
        if (!OS.IsLinux) throw new PlatformNotSupportedException();
        var targetDirectory = $"/srv/{Metadata.Name}";
        var bundleExtractDirectory = $"/var/tmp/{Metadata.User}-bundle-extract";
        if (Metadata.User is null) throw new InvalidOperationException(E.LinuxUserRequired);
        const string serviceBaseDir = "/srv";
        var serviceUserName = Metadata.User;
        var serviceGroupName = Metadata.Group ?? Metadata.User;
        await Linux.AddSystemUserAsync(serviceUserName, serviceGroupName, serviceBaseDir);
        Linux.AddSystemDirectory(targetDirectory, serviceUserName, serviceGroupName);
        Linux.AddSystemDirectory(bundleExtractDirectory, serviceUserName, serviceGroupName);
        var serviceUser = UserInfo.FromName(serviceUserName)!;
        var serviceGroup = GroupInfo.FromName(serviceGroupName)!;
        var dpapi = ApiResolver.GetNonWindowsDPAPI<IAcceptMessage>();
        dpapi?.Message((serviceUserName, serviceGroupName)); // configures DPAPI for the service user if applicable
        var isDll = Executable.FilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        FileSystem.CopyDirectoryContent(Executable.Directory.FullName, targetDirectory);
        Linux.ChownR(targetDirectory, serviceUser, serviceGroup);
        Linux.ChmodR(targetDirectory, "o-wX");
        var binaryPath = Path.Combine(targetDirectory, Executable.FileInfo.Name);
        if (!isDll) Linux.Chmod(binaryPath, "+x,o-wx");
        File.WriteAllLines($"/etc/systemd/system/{Metadata.Name}.service", [
            "[Unit]",
            $"Description={Metadata.DisplayName}",
            "",
            "[Service]",
            "Type=notify",
            $"Environment=\"DOTNET_BUNDLE_EXTRACT_BASE_DIR={bundleExtractDirectory}\"",
            $"ExecStart={(isDll ? $"dotnet {binaryPath}" : binaryPath)}",
            $"User={serviceUserName}",
            $"Group={serviceGroupName}",
            "",
            "[Install]",
            "WantedBy=multi-user.target"
        ]);
        if (Metadata.Start is StartType.Auto or StartType.DelayedAuto)
            await new ShellCommand($"systemctl enable {Metadata.Name}.service").ExecVoidAsync();
    }

    /// <summary>
    /// Deletes a systemd service from Linux OS.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the service is deleted.</returns>
    public async ValueTask DeleteSystemDServiceAsync() {
        await new ShellCommand($"systemctl disable {Metadata.Name}.service").ExecAndForgetAsync();
        try { File.Delete($"/etc/systemd/system/{Metadata.Name}.service"); } catch { }
        try { Directory.Delete($"/srv/{Metadata.Name}", recursive: true); } catch { }
        try { Directory.Delete($"/var/tmp/{Metadata.User}-bundle-extract/{Metadata.Name}"); } catch { }
    }

    private readonly ServiceMetadataSystemd Metadata;

}