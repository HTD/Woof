using Microsoft.Extensions.Configuration;

namespace Woof.ServiceInstaller;

/// <summary>
/// Defines a Windows Service metadata sufficient to register the service.
/// </summary>
public class ServiceMetadata {

    /// <summary>
    /// Gets the service's name (OS platform dependent). Sets the Windows Service name.
    /// Set the <see cref="LinuxName"/> for Linux.
    /// </summary>
    public string? Name {
        get => OS.IsWindows ? WindowsName : OS.IsLinux ? LinuxName : throw new PlatformNotSupportedException();
        set => WindowsName = value;
    }

    /// <summary>
    /// Gets or sets the service's display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the service's description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the service's start type.
    /// Should be one of the <see cref="StartType"/> values.
    /// </summary>
    public string? Start { get; set; } = StartType.Auto;

    /// <summary>
    /// Gets or sets the service's event log name.
    /// </summary>
    public string? EventLogName { get; set; }

    /// <summary>
    /// Gets or sets the service's event source name.
    /// </summary>
    public string? EventSourceName { get; set; }

    /// <summary>
    /// Gets or sets the minimal logging level for the service.
    /// </summary>
    public LogLevel EventLogLevelMinimal { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets or sets the name for the Linux service to be used for the directory and file names.
    /// </summary>
    public string? LinuxName { get; set; }

    /// <summary>
    /// Gets or sets the user name for the Linux systemd configuration.
    /// </summary>
    public string? LinuxUser { get; set; } = "service";

    /// <summary>
    /// Gets or sets the group name for the Linux systemd configuration.
    /// </summary>
    public string? LinuxGroup { get; set; } = "service";

    /// <summary>
    /// Windows name backing field.
    /// </summary>
    private string? WindowsName;

    /// <summary>
    /// Loads the metadata from <see cref="IConfiguration"/>.
    /// </summary>
    /// <remarks>
    /// The section name should be "Service". Point either to the section containing "Service" or to the "Service" section itself.
    /// </remarks>
    /// <typeparam name="TService">Service class type.</typeparam>
    /// <param name="configuration">Configuration containing "Service" section.</param>
    /// <returns>Service metadata.</returns>
    public static ServiceMetadata Load<TService>(IConfiguration configuration) where TService : class, IHostedService {
        const string sectionName = "Service";
        var serviceSection = configuration is IConfigurationSection section && section.Key == sectionName
            ? section
            : configuration.GetSection(sectionName);
        if (serviceSection is null) throw new NullReferenceException($"No {sectionName} in configuration");
        var properties = typeof(ServiceMetadata).GetProperties();
        var metadata = new ServiceMetadata();
        foreach (var property in properties) {
            var value = serviceSection.GetValue(property.PropertyType, property.Name);
            if (value is not null) property.SetValue(metadata, value);
        }
        return metadata;
    }

}
