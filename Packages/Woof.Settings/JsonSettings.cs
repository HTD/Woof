namespace Woof.Settings;

/// <summary>
/// Provides methods for loading and saving complex object from and to a program configuration file.
/// </summary>
/// <typeparam name="T">The type of configuration data.</typeparam>
public abstract class JsonSettings<T> : ISettings<T> where T : class {

    /// <summary>
    /// Loads the program configuration file.
    /// </summary>
    /// <returns>Configuration data.</returns>
    public virtual T Load() {
        (_Metadata.FilePath, var fileExists) = _Metadata.Locator.Locate(_Metadata.Name);
        if (!fileExists) return _Data;
        _Metadata.Root = _Metadata.Loader.Load(_Metadata.FilePath);
        _Metadata.Root.Bind(_Data!);
        return _Data;
    }

    /// <summary>
    /// Loads the program configuration file.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning configuration data when loaded.</returns>
    public virtual async ValueTask<T> LoadAsync() {
        (_Metadata.FilePath, var fileExists) = _Metadata.Locator.Locate(_Metadata.Name);
        if (!fileExists) return _Data;
        _Metadata.Root = await _Metadata.Loader.LoadAsync(_Metadata.FilePath);
        _Metadata.Root.Bind(_Data!);
        return _Data;
    }

    /// <summary>
    /// Saves the changes in configuration data.
    /// </summary>
    public void Save() {
        if (_Metadata.Root is null) _Metadata.Root = _Metadata.Loader.Parse("{}");
        _Metadata.Root.Set(_Data);
        var (primaryPath, secondaryPath) = GetLocations();
        try {
            _Metadata.Loader.Save(_Metadata.Root, primaryPath);
            _Metadata.FilePath = primaryPath;
        }
        catch (UnauthorizedAccessException) {
            _Metadata.Loader.Save(_Metadata.Root, secondaryPath);
            _Metadata.FilePath = secondaryPath;
        }
    }

    /// <summary>
    /// Saves the changes in configuration data.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when the saving is done.</returns>
    public async ValueTask SaveAsync() {
        if (_Metadata.Root is null) _Metadata.Root = _Metadata.Loader.Parse("{}");
        _Metadata.Root.Set(_Data);
        var (primaryPath, secondaryPath) = GetLocations();
        try {
            await _Metadata.Loader.SaveAsync(_Metadata.Root, primaryPath);
            _Metadata.FilePath = primaryPath;
        }
        catch (UnauthorizedAccessException) {
            await _Metadata.Loader.SaveAsync(_Metadata.Root, secondaryPath);
            _Metadata.FilePath = secondaryPath;
        }
    }

    /// <summary>
    /// Gets the primary and secondary locations for the file to save.
    /// If the path is already set, than it would be primary, locator preference ignored.
    /// </summary>
    /// <returns>Primary path to save the file, if it's not writeable, the secondary path can be tried then.</returns>
    public (string primaryPath, string secondaryPath) GetLocations() {
        var (primaryPath, secondaryPath) = _Metadata.Locator.LocateNew(_Metadata.Name);
        return _Metadata.FilePath is null || primaryPath.Equals(_Metadata.FilePath, StringComparison.Ordinal)
            ? (primaryPath, secondaryPath)
            : (secondaryPath, primaryPath);
    }

    /// <summary>
    /// Parses the configuration data from JSON text, for testing purpose.
    /// </summary>
    /// <param name="text">JSON text.</param>
    /// <returns>Configuration data.</returns>
    public T Parse(string text) {
        _Metadata.Root = _Metadata.Loader.Parse(text);
        _Metadata.Root.Bind(_Data!);
        return _Data;
    }

    /// <summary>
    /// Gets the configuration data.
    /// The configuration class must extend this class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declutter property name space for the target")]
    protected T _Data => (this as T)!;

    /// <summary>
    /// Gets the metadata for the instance.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Declutter property name space for the target")]
    protected virtual ISettingsMetadata<JsonNode> _Metadata { get; } = new JsonSettingsMetadata();

}