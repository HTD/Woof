namespace Woof.Settings;

/// <summary>
/// Contains settings metadata for for a <see cref="JsonNode"/> type.
/// </summary>
public class JsonSettingsMetadata : ISettingsMetadata<JsonNode> {

    /// <summary>
    /// Gets the module that locates the configuration file.
    /// </summary>
    public virtual ILocator Locator { get; } = new JsonSettingsLocator();

    /// <summary>
    /// Gets the module that loads and saves the data.
    /// </summary>
    public virtual ILoader<JsonNode> Loader { get; } = new JsonNodeLoader();

    /// <summary>
    /// Gets or sets the configuration file base name (no extension). Defaults to the application name.
    /// </summary>
    public string Name { get; set; } = Application.Name;

    /// <summary>
    /// Gets the path to the configuration file if it's loaded or saved. Null otherwise.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets the root node of the configuration data.
    /// </summary>
    public JsonNode? Root { get; set; }

}