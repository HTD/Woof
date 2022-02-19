namespace Woof.DataProtection;

/// <summary>
/// A base class for data protection key details.
/// </summary>
public abstract class DataProtectionKeyBase : IDataProtectionKey {

    /// <summary>
    /// Gets the data protector instance.
    /// </summary>
    public IDataProtector Protector { get; }

    /// <summary>
    /// Gets the <see cref="IDataProtectionProvider"/> instance.
    /// </summary>
    public IDataProtectionProvider Provider { get; }

    /// <summary>
    /// Gets the <see cref="IKey"/> instance.
    /// </summary>
    public IKey Key { get; }

    /// <summary>
    /// Gets the <see cref="IKeyManager"/> instance.
    /// </summary>
    public IKeyManager KeyManager { get; }

    /// <summary>
    /// Gets the key directory.
    /// </summary>
    public DirectoryInfo KeyDirectory { get; }

    /// <summary>
    /// Gets the full key file path.
    /// </summary>
    public FileInfo KeyFile { get;  }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    protected DataProtectionKeyBase(DataProtectionKeyConfiguration configuration) {
        Protector = configuration.Protector;
        Provider = configuration.Provider;
        Key = configuration.Key;
        KeyManager = configuration.KeyManager;
        KeyDirectory = configuration.KeyDirectory;
        KeyFile = configuration.KeyFile;
    }

    /// <summary>
    /// Gets the key configuration.
    /// </summary>
    /// <param name="keyDirectory">A directory that should contain data protection keys.</param>
    /// <param name="keyPurpose">The purpose to be assigned to the newly-created <see cref="IDataProtector"/>.</param>
    /// <returns>Key configuration for the base constructor.</returns>
    protected static DataProtectionKeyConfiguration GetConfiguration(string keyDirectory, string keyPurpose) {
        DataProtectionKeyConfiguration c = new();
        c.KeyDirectory = new DirectoryInfo(keyDirectory);
        c.Provider = CreateDataProtectionProvider(c.KeyDirectory, out var keyManager);
        c.KeyManager = keyManager;
        c.Protector = c.Provider.CreateProtector(keyPurpose);
        c.Protector.Protect(new byte[1]); // initializes the protector, forces key creation.
        c.Key = keyManager.GetAllKeys().Where(k => !k.IsRevoked).OrderByDescending(k => k.ActivationDate).First();
        c.KeyFile = new(Path.Combine(c.KeyDirectory.FullName, $"key-{c.Key!.KeyId}.xml"));
        return c;
    }

    /// <summary>
    /// Creates the default <see cref="IDataProtectionProvider"/> using internal builder to get the <see cref="IKeyManager"/> instance with it.
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://github.com/dotnet/aspnetcore/blob/main/src/DataProtection/Extensions/src/DataProtectionProvider.cs"/>.
    /// </remarks>
    /// <param name="keyDirectory">A directory that should contain data protection keys.</param>
    /// <param name="keyManager">The key manager instance.</param>
    /// <returns><see cref="IDataProtectionProvider"/> instance.</returns>
    protected static IDataProtectionProvider CreateDataProtectionProvider(DirectoryInfo keyDirectory, out IKeyManager keyManager) {
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddDataProtection();
        builder.PersistKeysToFileSystem(keyDirectory);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        keyManager = serviceProvider.GetService<IKeyManager>()!;
        return serviceProvider.GetRequiredService<IDataProtectionProvider>();
    }

}