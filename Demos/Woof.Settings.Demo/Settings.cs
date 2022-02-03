namespace Woof.Settings.Demo;

public class Settings : JsonSettingsAkv<Settings> {

    public Settings() : base(DataProtectionScope.CurrentUser) {
        _Metadata.Name = "Settings";
        _Metadata.Locator.PreferUserDirectory = true;
    }

    public Uri? Uri { get; init; }

    public IPAddress? Ip { get; init; }

    public byte[]? ExpectedKey { get; init; }

    public ProtectedData? ProtectedKey { get; init; }

    public ProtectedString? ProtectedString { get; init; }

    [AKV("TestKey")]
    public byte[]? AkvKey { get; init; }

}
