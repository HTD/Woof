namespace Woof.Settings;

/// <summary>
/// Contains settings metadata for a specific node type.
/// </summary>
/// <typeparam name="TNode">Node type.</typeparam>
public interface ISettingsMetadata<TNode> {

    /// <summary>
    /// Gets the module that locates the configuration file.
    /// </summary>
    ILocator Locator { get; }

    /// <summary>
    /// Gets the module that loads and saves the data.
    /// </summary>
    ILoader<TNode> Loader { get; }

    /// <summary>
    /// Gets or sets the configuration file base name (no extension).
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the path to the configuration file if it's loaded or saved. Null otherwise.
    /// </summary>
    string? FilePath { get; set; }

    /// <summary>
    /// Gets the root node of the configuration data.
    /// </summary>
    TNode? Root { get; set; }

}