namespace Woof.Config.Protected.Demo;

#pragma warning disable CS8618 // DTO
internal class Settings : JsonSettingsProtected<Settings> {

    public static Settings Default { get; } = new Settings();

    private Settings() : base(DataProtectionScope.CurrentUser) { }

    public string Login { get; init; }

    public byte[] ApiKey { get; init; }

}
#pragma warning restore CS8618