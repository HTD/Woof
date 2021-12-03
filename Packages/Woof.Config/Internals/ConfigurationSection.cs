namespace Woof.Config.Internals;

/// <summary>
/// Virtual, dead end configuration section to provide empty values.
/// </summary>
internal class ConfigurationSection : IConfigurationSection {

    /// <summary>
    /// Setting values here is not implemented.
    /// </summary>
    /// <param name="key">Key name.</param>
    /// <returns>Empty value.</returns>
    /// <exception cref="InvalidOperationException">Value set.</exception>
    public string this[string key] {
        get => "";
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the key this section occupies in its parent.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full path to this section within the <see cref="IConfiguration"/>.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the section value.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets the immediate descendant configuration sub-sections.
    /// </summary>
    /// <returns>Empty enumeration.</returns>
    public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>Exception.</returns>
    /// <exception cref="NotImplementedException">Invoked.</exception>
    public IChangeToken GetReloadToken() => throw new NotImplementedException();

    /// <summary>
    /// Gets a configuration sub-section with the specified key.
    /// </summary>
    /// <param name="key">The key of the configuration section.</param>
    /// <returns>Empty enumeration.</returns>
    public IConfigurationSection GetSection(string key) => Empty;

    /// <summary>
    /// Gets an empty <see cref="IConfigurationSection"/> value.
    /// </summary>
    public static IConfigurationSection Empty { get; } = new ConfigurationSection();

}