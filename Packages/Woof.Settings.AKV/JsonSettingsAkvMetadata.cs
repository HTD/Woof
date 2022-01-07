namespace Woof.Settings;

/// <summary>
/// Contains settings metadata for for a <see cref="JsonNode"/> type, including the <see cref="VaultConfiguration"/>.
/// </summary>
public class JsonSettingsAkvMetadata : JsonSettingsMetadata {

    /// <summary>
    /// Gets the configuration for the Azure Key Vault.
    /// </summary>
    public DataProtectionScope ProtectionScope { get; }

    /// <summary>
    /// Configures the data protection scope for the AKV configuration file.
    /// </summary>
    /// <param name="protectionScope"></param>
    public JsonSettingsAkvMetadata(DataProtectionScope protectionScope) => ProtectionScope = protectionScope;

}