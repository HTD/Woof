namespace Woof.DataProtection.Api;

/// <summary>
/// Linux data protection API.
/// </summary>
public class DPAPI_Linux : IDPAPI {

    /// <summary>
    /// Default system directory for storing Linux application keys.
    /// </summary>
    public const string DefaultLinuxKeyDirectorySystem = "/etc/dotnet/dpapi";

    /// <summary>
    /// Default user directory for storing Linux application keys.
    /// </summary>
    public const string DefaultLinuxKeyDirectoryUser = "~/.net/dpapi";

    /// <summary>
    /// Gets or sets the protection key directory.
    /// </summary>
    /// <remarks>
    /// The directory will contain application key created on first run.<br/>
    /// It is important that the directory must be writeable for the application on first run and must not be accessible for unauthorized users.<br/>
    /// When the key is lost, the access to the protected data is lost with the key.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Directory is null.</exception>
    public string? KeyDirectory {
        get => _KeyDirectory;
        set {
            if (value is null) throw new InvalidOperationException("Key directory cannot be null");
            if (_KeyDirectory == value) return;
            _KeyDirectory = value;
            Provider = CreateDataProtectionProvider(new DirectoryInfo(value));
            _Protector = Provider.CreateProtector("Woof.DPAPI");
            // IsOperational read will trigger key file creation if not already created, this will allow to change the user key ownership:
            var keyUser = CurrentUser.BehindSudo ?? Linux.CurrentProcessUser;
            if (keyUser is not null && IsOperational) Linux.ChownR(value, keyUser);
            // The read access to the key must be limited to the owning user only.
            RestrictKeyAccess();
        }
    }

    /// <summary>
    /// Gets the key manager if the KeyDirectory was set, null otherwise.
    /// </summary>
    public IKeyManager? KeyManager { get; private set; }

    /// <summary>
    /// Gets a value indicating that the data protection service is configured (the <see cref="KeyDirectory"/> is set and the protection key exits).
    /// </summary>
    public bool IsConfigured => Provider is not null;

    /// <summary>
    /// Gets a value indicating that the data protection service is operational.<br/>
    /// If false is returned, it indicates the program doesn't have sufficient access to the key directory.<br/>
    /// This happens when the key directory and key file cannot be read by the program.<br/>
    /// This also happens when the program is first run, the key file does not exist and the program can't write to the key directory.
    /// </summary>
    public bool IsOperational {
        get {
            if (Provider is null) AutoConfigure();
            try {
                _ = Protector.Protect(new byte[1]);
            }
            catch (CryptographicException) { return false; }
            return true;
        }
    }

    /// <summary>
    /// Gets the <see cref="IDataProtectionProvider"/> for the <see cref="KeyDirectory"/>.
    /// </summary>
    /// <remarks>
    /// Set the <see cref="KeyDirectory"/> first, or get null.
    /// </remarks>
    public IDataProtectionProvider? Provider { get; private set; }

    /// <summary>
    /// Gets the <see cref="IDataProtector"/> instance that can provide data protection services for the <see cref="KeyDirectory"/>.<br/>
    /// To use Windows DPAPI that doesn't require <see cref="KeyDirectory"/> use Protect / Unprotect methods instead.
    /// </summary>
    /// <remarks>
    /// Set the <see cref="KeyDirectory"/> first, or get <see cref="NullReferenceException"/>.
    /// </remarks>
    /// <exception cref="NullReferenceException"><see cref="KeyDirectory"/> is null.</exception>
    public IDataProtector Protector {
        get {
            if (_Protector is null) AutoConfigure();
            return _Protector ?? throw new NullReferenceException("Set the KeyDirectory before getting the Protector");
        }
    }

    /// <summary>
    /// Tries to automatically configure the application data protection key storage to a default target directory.
    /// </summary>
    /// <param name="dataProtectionScope">Optional data protection scope.</param>
    public void AutoConfigure(DataProtectionScope? dataProtectionScope = null) {
        if (dataProtectionScope == DataProtectionScope.LocalMachine) {
            KeyDirectory = DefaultLinuxKeyDirectorySystem;
            return;
        }
        KeyDirectory = Linux.ResolveUserPath(DefaultLinuxKeyDirectoryUser);
        // FIXME: ADD OPTION TO CONFIGURE THE KEY DIRECTORY FOR THE SPECIFIED USER!
    }

    /// <summary>
    /// Restricts the key access for specified user or current user (including sudo users).
    /// Linux only, does nothing on Windows.
    /// </summary>
    /// <param name="user">User information or null, to use current user.</param>
    /// <param name="group">Optional group the access will be granted to.</param>
    public void RestrictKeyAccess(UserInfo? user = null, GroupInfo? group = null) {
        if (user is null) user = CurrentUser.BehindSudo ?? Linux.CurrentProcessUser;
        if (KeyDirectory is null) AutoConfigure(); // FIXME: ADD OPTION TO CONFIGURE THE KEY DIRECTORY FOR THE SPECIFIED USER!
        var currentKey = KeyManager!.GetAllKeys().Where(k => !k.IsRevoked).OrderByDescending(k => k.ActivationDate).FirstOrDefault();
        var keyFileName = $"key-{currentKey!.KeyId}.xml";
        var keyPath = Path.Combine(KeyDirectory!, keyFileName);
        if (group is not null) {
            Linux.Chown(KeyDirectory!, user.Uid, group.Id);
            Linux.Chown(keyPath, user.Uid, group.Id);
            Linux.Chmod(keyPath, "g+rw");
        }
        Linux.Chmod(keyPath, "o-rwx");
    }

    /// <summary>
    /// Encrypts data.
    /// </summary>
    /// <param name="data">Raw data.</param>
    /// <param name="scope">Ignored.</param>
    /// <returns>Protected data.</returns>
    public byte[] Protect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser) {
        AutoConfigure(scope);
        return Protector.Protect(data);
    }

    /// <summary>
    /// Decrypts the data.
    /// </summary>
    /// <param name="data">Protected data.</param>
    /// <param name="scope">Ignored.</param>
    /// <returns>Decrypted data.</returns>
    public byte[] Unprotect(byte[] data, DataProtectionScope scope = DataProtectionScope.CurrentUser) {
        AutoConfigure(scope);
        return Protector.Unprotect(data);
    }

    /// <summary>
    /// Creates the default <see cref="IDataProtectionProvider"/> using internal builder to get the <see cref="IKeyManager"/> instance with it.
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://github.com/dotnet/aspnetcore/blob/main/src/DataProtection/Extensions/src/DataProtectionProvider.cs"/>.
    /// </remarks>
    /// <param name="keyDirectory">A directory that should contain data protection keys.</param>
    /// <returns><see cref="IDataProtectionProvider"/> instance.</returns>
    private IDataProtectionProvider CreateDataProtectionProvider(DirectoryInfo keyDirectory) {
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddDataProtection();
        builder.PersistKeysToFileSystem(keyDirectory);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        KeyManager = serviceProvider.GetService<IKeyManager>();
        return serviceProvider.GetRequiredService<IDataProtectionProvider>();
    }

    private static string? _KeyDirectory;
    private static IDataProtector? _Protector;

}