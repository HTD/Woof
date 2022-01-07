namespace Woof.Settings;

/// <summary>
/// Locates the configuration files.
/// </summary>
public class JsonSettingsProtectedLocator : JsonSettingsLocator {

    /// <summary>
    /// Gets the supported configuration file extensions. Derived class can add its own.
    /// </summary>
    protected override IEnumerable<string> Extensions => new[] { SettingsProtected.Extension, ".json" };

}