using System.Security.Cryptography;

namespace Woof.Settings;

/// <summary>
/// Provides access to the Azure Key Vault.
/// </summary>
public class AkvAccessProvider {

    /// <summary>
    /// Creates a new instance of <see cref="AkvAccessProvider"/>.
    /// </summary>
    /// <param name="configuration">Azure Key Vault access configuration.</param>
    public AkvAccessProvider(VaultConfiguration configuration) => Configuration = configuration;

    /// <summary>
    /// Gets the secret value as byte array from the Azure Key Vault.
    /// </summary>
    /// <param name="name">Secret name.</param>
    /// <returns>Data decoded from Base64 string.</returns>
    public byte[] GetBytes(string name)
        => ByteCache.ContainsKey(name) ? ByteCache[name] : (ByteCache[name] = Convert.FromBase64String(SecretClient.GetSecret(name).Value.Value));

    /// <summary>
    /// Gets the secret value from the Azure Key Vault.
    /// </summary>
    /// <param name="name">Secret name.</param>
    /// <returns>Secret string.</returns>
    public string GetString(string name)
        => StringCache.ContainsKey(name) ? StringCache[name] : (StringCache[name] = SecretClient.GetSecret(name).Value.Value);

    /// <summary>
    /// Encrypts a string with the specified key.
    /// </summary>
    /// <param name="keyName">Key name.</param>
    /// <param name="value">String to encrypt.</param>
    /// <returns>Initialization vector and encrypted data.</returns>
    public (byte[], byte[]) Encrypt(string keyName, string value) {
        var data = Encoding.UTF8.GetBytes(value);
        Aes.Key = GetBytes(keyName);
        Aes.GenerateIV();
        using var encryptor = Aes.CreateEncryptor();
        return (Aes.IV, encryptor.TransformFinalBlock(data, 0, data.Length));
    }

    /// <summary>
    /// Decrypts encrypted string with the specified key.
    /// </summary>
    /// <param name="keyName">Key name.</param>
    /// <param name="iV">Initialization vector.</param>
    /// <param name="data">Encrypted data.</param>
    /// <returns>Decrypted string.</returns>
    public string Decrypt(string keyName, byte[] iV, byte[] data) {
        using var decryptor = Aes.CreateDecryptor(GetBytes(keyName), iV);
        return Encoding.UTF8.GetString(decryptor.TransformFinalBlock(data, 0, data.Length));
    }

    /// <summary>
    /// Encrypts a type containing credentials.<br/>
    /// The return type should have an additional property matching the source type property name ending with "IV" string.
    /// </summary>
    /// <param name="decrypted">Type containing non-encrypted credentials as strings.</param>
    /// <returns>Type containing encrypted credentials as byte arrays.</returns>
    public TEncrypted EncryptCredentials<TDecrypted, TEncrypted>(TDecrypted decrypted) where TEncrypted : new() {
        if (Configuration.CredentialsEncodingKeyName is null) throw new InvalidOperationException(ENoEncodingKeySet);
        if (decrypted is null) throw new ArgumentNullException(nameof(decrypted));
        var encrypted = new TEncrypted();
        var decryptedProperties = decrypted.GetType().GetProperties();
        var encryptedProperties = encrypted.GetType().GetProperties();
        var encryptedPropertyNames = encryptedProperties.Where(p => p.Name.EndsWith("IV")).Select(p => p.Name[0..^2]);
        foreach (var decryptedProperty in decryptedProperties) {
            var name = decryptedProperty.Name;
            var encryptedProperty = encryptedProperties.First(p => p.Name == name);
            if (encryptedPropertyNames.Contains(encryptedProperty.Name)) {
                var decryptedString = decryptedProperty.GetValue(decrypted) as string ?? throw new NullReferenceException();
                (byte[] iv, byte[] data) = Encrypt(Configuration.CredentialsEncodingKeyName, decryptedString);
                var encryptedIvProperty = encryptedProperties.First(p => p.Name == $"{name}IV");
                encryptedIvProperty.SetValue(encrypted, iv);
                encryptedProperty.SetValue(encrypted, data);
            }
            else encryptedProperty.SetValue(encrypted, decryptedProperty.GetValue(decrypted));
        }
        return encrypted;
    }

    /// <summary>
    /// Decrypts a type containing credentials.<br/>
    /// The source type should have an additional property matching the return type property name ending with "IV" string.
    /// </summary>
    /// <param name="encrypted">Type containing encrypted credentials as byte arrays.</param>
    /// <returns>Type containing decrypted credentials as strings.</returns>
    public TDecrypted DecryptCredentials<TEncrypted, TDecrypted>(TEncrypted encrypted) where TDecrypted : new() {
        if (Configuration.CredentialsEncodingKeyName is null) throw new InvalidOperationException(ENoEncodingKeySet);
        if (encrypted is null) throw new ArgumentNullException(nameof(encrypted));
        var decrypted = new TDecrypted();
        var decryptedProperties = decrypted.GetType().GetProperties();
        var encryptedProperties = encrypted.GetType().GetProperties();
        var encryptedPropertyNames = encryptedProperties.Where(p => p.Name.EndsWith("IV")).Select(p => p.Name[0..^2]);
        foreach (var decryptedProperty in decryptedProperties) {
            var name = decryptedProperty.Name;
            var encryptedProperty = encryptedProperties.First(p => p.Name == name);
            if (encryptedPropertyNames.Contains(encryptedProperty.Name)) {
                var encryptedIvProperty = encryptedProperties.First(p => p.Name == $"{name}IV");
                var iv = (byte[])(encryptedIvProperty.GetValue(encrypted) ?? throw new NullReferenceException());
                var data = (byte[])(encryptedProperty.GetValue(encrypted) ?? throw new NullReferenceException());
                var decryptedString = Decrypt(Configuration.CredentialsEncodingKeyName, iv, data);
                decryptedProperty.SetValue(decrypted, decryptedString);
            }
            else decryptedProperty.SetValue(decrypted, encryptedProperty.GetValue(encrypted));
        }
        return decrypted;
    }

    /// <summary>
    /// Gets the vault credentials from configuration.
    /// </summary>
    private ClientSecretCredential ClientSecretCredential
        => _ClientSecretCredential ??=
            new ClientSecretCredential(Configuration.DirectoryId, Configuration.ApplicationId, Configuration.ClientSecret);

    /// <summary>
    /// Gets the AKV secret client.
    /// </summary>
    private SecretClient SecretClient
        => _SecretClient ??= new SecretClient(Configuration.VaultUri, ClientSecretCredential);

    /// <summary>
    /// Azure Key Vault configuration.
    /// </summary>
    private readonly VaultConfiguration Configuration;

    /// <summary>
    /// <see cref="ClientSecretCredential"/> backing field.
    /// </summary>
    private ClientSecretCredential? _ClientSecretCredential;

    /// <summary>
    /// <see cref="SecretClient"/> backing field.
    /// </summary>
    private SecretClient? _SecretClient;

    /// <summary>
    /// A cache for string values.
    /// </summary>
    private readonly Dictionary<string, string> StringCache = new();

    /// <summary>
    /// A cache for binary values.
    /// </summary>
    private readonly Dictionary<string, byte[]> ByteCache = new();

    /// <summary>
    /// Local AES algorithm instance.
    /// </summary>
    private readonly Aes Aes = Aes.Create();

    const string ENoEncodingKeySet = $"The {nameof(VaultConfiguration.CredentialsEncodingKeyName)} property is not set in access configuration";

}