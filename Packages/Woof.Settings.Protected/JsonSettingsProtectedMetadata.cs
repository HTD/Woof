namespace Woof.Settings;

/// <summary>
/// Contains settings metadata using data protection for for a <see cref="JsonNode"/> type.
/// </summary>
public class JsonSettingsProtectedMetadata : JsonSettingsMetadata {

    /// <summary>
    /// Gets the module that locates the protected configuration file.
    /// </summary>
    public override ILocator Locator { get; }

    /// <summary>
    /// Gets the module that loads and saves the protected data.
    /// </summary>
    public override ILoader<JsonNode> Loader { get; }

    /// <summary>
    /// Creates the metadata for the protection scope.
    /// </summary>
    /// <param name="protectionScope">Data protection scope for the loader.</param>
    public JsonSettingsProtectedMetadata(DataProtectionScope? protectionScope) {
        Locator = new JsonSettingsProtectedLocator();
        Loader = new JsonNodeProtectedLoader(protectionScope);
    }

}