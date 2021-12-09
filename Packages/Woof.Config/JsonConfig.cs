namespace Woof.Config;

/// <summary>
/// JSON file <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Implements the default <see cref="IFileLocator"/> and <see cref="IJsonNodeLoader"/>.
/// </remarks>
public class JsonConfig : JsonNodeSection {

    /// <summary>
    /// Gets the <see cref="IFileLocator"/> instance used to locate the file used to create this configuration.
    /// </summary>
    public IFileLocator Locator { get; }

    /// <summary>
    /// Gets the <see cref="IJsonNodeLoader"/> instance used to load this configuration.
    /// </summary>
    public IJsonNodeLoader Loader { get; }

    /// <summary>
    /// Creates the configuration with a specified name or a default one, using application name.
    /// </summary>
    /// <param name="name">Configuration name, when not set the application name will be used.</param>
    public JsonConfig(string? name = null) : this(name, new JsonConfigLocator(), new JsonNodeLoader()) { }

    /// <summary>
    /// Creates the configuration from the root node.
    /// </summary>
    /// <param name="rootNode">Root node.</param>
    /// <param name="locator">A <see cref="IFileLocator"/> instance.</param>
    /// <param name="loader">A <see cref="IJsonNodeLoader"/> instance.</param>
    protected JsonConfig(JsonNode rootNode, IFileLocator locator, IJsonNodeLoader loader) : base(rootNode) {
        Locator = locator;
        Loader = loader;
    }

    /// <summary>
    /// Loads the configuration. Creates the file in the user directory if doesn't exist.
    /// </summary>
    /// <param name="name">Configuration name. If null, application name will be used.</param>
    /// <param name="locator">A <see cref="IFileLocator"/> instance.</param>
    /// <param name="loader">A <see cref="IJsonNodeLoader"/> instance.</param>
    protected JsonConfig(string? name, IFileLocator locator, IJsonNodeLoader loader) : base(LoadRootOrCreateEmpty(name, locator, loader)) {
        Locator = locator;
        Loader = loader;
    }

    /// <summary>
    /// Loads a new configuration instance.
    /// </summary>
    /// <param name="name">Configuration name. If null, application name will be used.</param>
    /// <returns>A task completed when the configuration file is loaded or created.</returns>
    public static async ValueTask<JsonConfig> LoadAsync(string? name = null) {
        var locator = new JsonConfigLocator();
        var loader = new JsonNodeLoader();
        var path = locator.Locate(name);
        return new JsonConfig(File.Exists(path) ? (await loader.LoadRootAsync(path))! : JsonNode.Parse("{}")!, locator, loader);
    }

    /// <summary>
    /// Loads the root node from file or creates an empty root.
    /// </summary>
    /// <param name="name">Configuration name, when not set the application name will be used.</param>
    /// <param name="locator">A <see cref="IFileLocator"/> instance.</param>
    /// <param name="loader">A <see cref="IJsonNodeLoader"/> instance.</param>
    /// <returns>Root node.</returns>
    protected static JsonNode LoadRootOrCreateEmpty(string? name, IFileLocator locator, IJsonNodeLoader loader) {
        var path = locator.Locate(name);
        return File.Exists(path) ? loader.LoadRoot(path)! : JsonNode.Parse("{}")!;
    }

}