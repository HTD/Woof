namespace Woof.Settings;

/// <summary>
/// Provides methods for loading and saving complex object from and to a program configuration file.<br/>
/// Some property values can be downloaded from the Azure Key Vault.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class JsonSettingsAkv<T> : JsonSettings<T> where T : class {

    /// <summary>
    /// Gets the configured access provider for the Azure Key Vault.
    /// </summary>
    public AkvAccessProvider AccessProvider
        => _AccessProvider ?? throw new InvalidOperationException("Access provider is not available before the configuration is loaded");

    /// <summary>
    /// Configures the data protection scope for the AKV access file.
    /// </summary>
    /// <param name="protectionScope">Data protection scope for the AKV access file.</param>
    protected JsonSettingsAkv(DataProtectionScope protectionScope) {
        _Metadata = new(protectionScope);
        SpecialAttribute.Resolve += SpecialAttributeResolver;
    }

    /// <summary>
    /// Loads the program settings file.
    /// </summary>
    /// <returns>Configuration data.</returns>
    public override T Load() {
        if (_AccessProvider is null)
            _AccessProvider = new(new VaultConfiguration(_Metadata.Name + ".access", _Metadata.ProtectionScope).Load().Protect());
        return base.Load();
    }

    /// <summary>
    /// Loads the program configuration file.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> returning configuration data when loaded.</returns>
    public async override ValueTask<T> LoadAsync() {
        if (_AccessProvider is null) {
            var vaultConfiguration = await new VaultConfiguration(_Metadata.Name + ".access", _Metadata.ProtectionScope).LoadAsync();
            await vaultConfiguration.ProtectAsync();
            _AccessProvider = new(vaultConfiguration);
        }
        return await base.LoadAsync();
    }

    /// <summary>
    /// Resolves the special properties decorated with the <see cref="AKVAttribute"/>.
    /// </summary>
    /// <param name="sender">The property attribute.</param>
    /// <param name="e">Event arguments to get the type from and set value to.</param>
    private void SpecialAttributeResolver(object? sender, SpecialAttributeEventArgs e) {
        if (sender is not AKVAttribute akv || akv.Name is null) return;
        var akvString = AccessProvider.GetString(akv.Name);
        e.Value = ValueConversions.Parse(akvString, e.Type);
    }

    /// <summary>
    /// Gets the metadata for the instance.
    /// </summary>
    protected override JsonSettingsAkvMetadata _Metadata { get; }

    /// <summary>
    /// <see cref="AccessProvider"/> backing field.
    /// </summary>
    private AkvAccessProvider? _AccessProvider;

}