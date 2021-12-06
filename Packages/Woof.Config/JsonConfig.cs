namespace Woof.Config;

/// <summary>
/// JSON file <see cref="IConfiguration"/>.
/// </summary>
/// <remarks>
/// Implements the default <see cref="IFileLocator"/> and <see cref="IJsonNodeLoader"/>.
/// </remarks>
public class JsonConfig : JsonNodeSection {

    /// <summary>
    /// Gets the <see cref="IFileLocator"/> for the configuration.
    /// </summary>
    public IFileLocator Locator { get; }

    /// <summary>
    /// Creates the configuration with a specified name or a default one, using application name.
    /// </summary>
    /// <param name="name">Configuration name, when not set the application name will be used.</param>
    public JsonConfig(string? name = null) : this(name, new JsonConfigLocator(), new JsonNodeLoader()) { }

    /// <summary>
    /// Loads the configuration. Creates the file in the user directory if doesn't exist.
    /// </summary>
    /// <param name="name">Configuration name. If null, application name will be used.</param>
    /// <param name="locator">A <see cref="IFileLocator"/> instance.</param>
    /// <param name="loader">A <see cref="IJsonNodeLoader"/> instance.</param>
    protected JsonConfig(string? name, IFileLocator locator, IJsonNodeLoader loader) : base(loader.LoadRoot(locator.Locate(name))!) {
        Locator = locator;
        Loader = loader;
    }

}