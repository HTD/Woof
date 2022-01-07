namespace Woof.ServiceInstaller.TestService;

public class Settings : JsonSettings<Settings> {

    public static Settings Default { get; } = new Settings();

    public ServiceMetadataWindows WindowsService { get; } = new();

    public ServiceMetadataSystemd SystemDaemon { get; } = new();

}