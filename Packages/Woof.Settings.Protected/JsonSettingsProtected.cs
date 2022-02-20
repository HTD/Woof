namespace Woof.Settings;

/// <summary>
/// Provides methods for loading and saving complex object from and to a protected program configuration file.
/// </summary>
/// <typeparam name="T">The type of configuration data.</typeparam>
public abstract class JsonSettingsProtected<T> : JsonSettings<T> where T : class {

    /// <summary>
    /// Metadata for the instance.
    /// </summary>
    protected override ISettingsMetadata<JsonNode> _Metadata { get; }

    /// <summary>
    /// Defines the protection scope for the settings.
    /// </summary>
    /// <param name="protectionScope">Protection scope. Use null for no protection, if it makes sense.</param>
    protected JsonSettingsProtected(DataProtectionScope protectionScope)
        => _Metadata = new JsonSettingsProtectedMetadata(DPAPI.AssertValidWindowsScope(protectionScope));

    /// <summary>
    /// Protects the settings file if it is loaded from the unprotected source.
    /// </summary>
    /// <returns>Settings data.</returns>
    public T Protect() {
        if (_Metadata.FilePath is null || Path.GetExtension(_Metadata.FilePath) == SettingsProtected.Extension) return _Data;
        var originalPath = _Metadata.FilePath;
        var protectedPath = Path.ChangeExtension(_Metadata.FilePath, SettingsProtected.Extension);
        _Metadata.FilePath = protectedPath;
        Save();
        File.Delete(originalPath);
        return _Data;
    }

    /// <summary>
    /// Protects the settings file if it is loaded from the unprotected source.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning settings data and completed when the protection is done.</returns>
    public async ValueTask<T> ProtectAsync() {
        if (_Metadata.FilePath is null || Path.GetExtension(_Metadata.FilePath) == SettingsProtected.Extension) return _Data;
        var originalPath = _Metadata.FilePath;
        var protectedPath = Path.ChangeExtension(_Metadata.FilePath, SettingsProtected.Extension);
        _Metadata.FilePath = protectedPath;
        await SaveAsync();
        File.Delete(originalPath);
        return _Data;
    }

}