namespace Woof.DataProtection.Api;

/// <summary>
/// Linux data protection api key for a user or system.<br/>
/// In order to use <see cref="DataProtectionScope.LocalMachine"/> this must be run via sudo first time.
/// </summary>
internal class DPAPI_LinuxKey {

    /// <summary>
    /// Gets the key owner.
    /// </summary>
    public UserInfo User { get; }

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
    public IKeyManager KeyManager { get; private set; }

    /// <summary>
    /// Gets the key directory.
    /// </summary>
    public DirectoryInfo KeyDirectory { get; }

    /// <summary>
    /// Gets the full key file path.
    /// </summary>
    public FileInfo KeyFile { get; }

    /// <summary>
    /// Gets the dictionary of the currenly configured keys for users.<br/>
    /// The entry at zero is the root entry - a key that can only be configured by root,
    /// but readable to all users when using <see cref="DataProtectionScope.LocalMachine"/>.
    /// </summary>
    internal static Dictionary<int, DPAPI_LinuxKey> Available { get; } = new();

    /// <summary>
    /// Gets the data protection key for the current user scope.
    /// </summary>
    public static DPAPI_LinuxKey CurrentUserScope {
        get {
            var user = CurrentUser.BehindSudo ?? UserInfo.CurrentProcessUser;
            return Available.ContainsKey(user.Uid) ? Available[user.Uid] : new DPAPI_LinuxKey(user);
        }
    }

    /// <summary>
    /// Gets the data protection key for the local machine scope.
    /// </summary>
    public static DPAPI_LinuxKey LocalMachineScope
        => Available.ContainsKey(0) ?
            Available[0] : CurrentUser.IsRoot ?
            new DPAPI_LinuxKey(UserInfo.FromUid(0)!) :
            throw new UnauthorizedAccessException("This must be run with sudo for the first time");

    /// <summary>
    /// Default system directory for storing Linux application keys.
    /// </summary>
    public const string SystemDirectory = "/etc/dotnet/dpapi";

    /// <summary>
    /// Default user directory for storing Linux application keys.
    /// </summary>
    public const string UserDirectory = "~/.net/dpapi";

    /// <summary>
    /// Configures the data protection key for the specified user and group.
    /// </summary>
    /// <param name="user">A user to create the data protection for.</param>
    /// <param name="group">A group the user belongs to, taken from user information if not set.</param>
    public DPAPI_LinuxKey(UserInfo user, GroupInfo? group = null) {
        User = user;
        if (group is null) group = GroupInfo.FromGid(user.Gid);
        KeyDirectory = new DirectoryInfo(user.Uid == 0 ? SystemDirectory : Linux.ResolveUserPath(UserDirectory));
        Provider = CreateDataProtectionProvider(KeyDirectory);
        Protector = Provider.CreateProtector($"Woof.DPAPI:{user.Name}");
        Protector.Protect(new byte[1]); // initializes the protector, forces key creation.
        Key = KeyManager.GetAllKeys().Where(k => !k.IsRevoked).OrderByDescending(k => k.ActivationDate).First();
        KeyFile = new(Path.Combine(KeyDirectory.FullName, $"key-{Key!.KeyId}.xml"));
        if (CurrentUser.IsRoot) Linux.ChownR(KeyDirectory.FullName, user, group!);
        if (user == UserInfo.CurrentProcessUser || CurrentUser.IsRoot)
            Linux.Chmod(KeyFile.FullName, user.Uid == 0 ? "666" : "660");
        Available[user.Uid] = this;
    }

    /// <summary>
    /// Configures the system wide <see cref="DataProtectionScope.LocalMachine"/> key for root.
    /// </summary>
    static DPAPI_LinuxKey() {
        try { // if the root key doesn't exist and the user has no root privileges, the next line fails...
            _ = new DPAPI_LinuxKey(UserInfo.FromUid(0)!);
        }
        catch { } // ...but the users should still be able to configure their own keys.
    }

    /// <summary>
    /// Creates the default <see cref="IDataProtectionProvider"/> using internal builder to get the <see cref="IKeyManager"/> instance with it.
    /// </summary>
    /// <remarks>
    /// Based on <see href="https://github.com/dotnet/aspnetcore/blob/main/src/DataProtection/Extensions/src/DataProtectionProvider.cs"/>.
    /// </remarks>
    /// <param name="keyDirectory">A directory that should contain data protection keys.</param>
    /// <returns><see cref="IDataProtectionProvider"/> instance.</returns>
    [MemberNotNull("KeyManager")]
    private IDataProtectionProvider CreateDataProtectionProvider(DirectoryInfo keyDirectory) {
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddDataProtection();
        builder.PersistKeysToFileSystem(keyDirectory);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        KeyManager = serviceProvider.GetService<IKeyManager>()!;
        return serviceProvider.GetRequiredService<IDataProtectionProvider>();
    }

}
