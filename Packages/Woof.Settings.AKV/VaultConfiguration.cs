namespace Woof.Settings;

#pragma warning disable CS8618

/// <summary>
/// Azure Key Vault configuration.
/// </summary>
public class VaultConfiguration : JsonSettingsProtected<VaultConfiguration> {

    /// <summary>
    /// Sets up the vault configuration name and data protection.
    /// </summary>
    /// <param name="name">The base name of the vault configuration file.</param>
    /// <param name="protectionScope">Protection scope for the vault configuration file.</param>
    internal VaultConfiguration(string name, DataProtectionScope protectionScope) : base(protectionScope) => _Metadata.Name = name;

    /// <summary>
    /// Gets the AKV URI.
    /// </summary>
    public Uri VaultUri { get; init; }

    /// <summary>
    /// Gets the DirectoryId / TennantId.
    /// </summary>
    public string DirectoryId { get; init; }

    /// <summary>
    /// Gets the ApplicationId / ClientId.
    /// </summary>
    public string ApplicationId { get; init; }

    /// <summary>
    /// Gets the client secret.
    /// </summary>
    public string ClientSecret { get; init; }

    /// <summary>
    /// Gets the name of the secret containing credentials encoding key.
    /// </summary>
    public string? CredentialsEncodingKeyName { get; init; }

}

#pragma warning restore CS8618