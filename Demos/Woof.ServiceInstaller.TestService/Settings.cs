namespace Woof.ServiceInstaller.TestService;

public class Settings : JsonSettingsAkv<Settings> {

    private Settings() : base(DataProtection.DataProtectionScope.LocalMachine) => DPAPI.UseServiceAPI = true;

    public static Settings Default { get; } = new Settings();

    public ServiceMetadataWindows WindowsService { get; } = new();

    public ServiceMetadataSystemd SystemDaemon { get; } = new();

}