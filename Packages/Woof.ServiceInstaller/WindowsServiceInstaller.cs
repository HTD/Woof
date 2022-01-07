namespace Woof.ServiceInstaller;

internal class WindowsServiceInstaller {

    /// <summary>
    /// Initializes the instance.
    /// </summary>
    /// <param name="serviceMetadata">Service medatada.</param>
    /// <exception cref="ArgumentException">Metadata Name property is null.</exception>
    /// <exception cref="ArgumentNullException">Argument is null.</exception>
    public WindowsServiceInstaller(ServiceMetadataWindows serviceMetadata) {
        if (!OS.IsWindows) throw new PlatformNotSupportedException();
        if (serviceMetadata is null) throw new ArgumentNullException(nameof(serviceMetadata));
        if (serviceMetadata.Name is null || serviceMetadata.Name.Length < 1)
            throw new ArgumentException(E.ServiceNameRequired, nameof(serviceMetadata));
        Metadata = serviceMetadata;
    }

    /// <summary>
    /// Registers a Windows Service in Windows OS.
    /// </summary>
    /// <returns><see cref="ValueTask"/> completed when the service is registered or an exception is thrown.</returns>
    public async ValueTask RegisterWindowsServiceAsync() {
        if (!OS.IsWindows) throw new PlatformNotSupportedException();
        var argsList = new List<string> {
                "create",
                Metadata.Name!,
                $"binPath={Application.Path}"
            };
        if (Metadata.DisplayName is not null) argsList.Add($"DisplayName={Metadata.DisplayName}");
        if (Metadata.Start != null) argsList.Add($"start={Metadata.Start}");
        await new ShellCommand("sc", argsList).ExecVoidAsync();
        if (Metadata.Description != null) await new ShellCommand($"sc description {Metadata.Name} {Metadata.Description}").ExecVoidAsync();
    }

    private readonly ServiceMetadataWindows Metadata;

}