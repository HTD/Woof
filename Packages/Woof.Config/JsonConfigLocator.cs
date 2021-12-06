namespace Woof.Config;

/// <summary>
/// Locates the configuration files.
/// </summary>
public class JsonConfigLocator : IFileLocator {

    /// <summary>
    /// Gets a value indicating that the entry assembly was built with the Debug configuration.
    /// </summary>
    public static bool IsDebug { get; }
        = Assembly.GetEntryAssembly()!.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);

    /// <summary>
    /// Gets the supported configuration file extensions. Derived class can add its own.
    /// </summary>
    protected virtual IEnumerable<string> Extensions => new[] { ".json" };

    /// <summary>
    /// Gets the path to the file located with the last <see cref="Locate(string?)"/> call.
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Gets the %LOCALAPPDATA% target folder for the current Windows user.
    /// </summary>
    /// <remarks>
    /// In order for for this to work with Microsoft Installer Projects,<br/>
    /// the installer must put the the program files in [ProgramFiles64Folder]\[Manufacturer]\[ProductName]<br/>
    /// and the configuration files in [LocalAppDataFolder]\[Manufacturer]\[ProductName].<br/><br/>
    /// The installer folders are matched as follows:<br/>
    /// - [LocalAppDataFolder] => %LOCALAPPDATA%,<br/>
    /// - [Manufacturer] => <see cref="AssemblyCompanyAttribute.Company"/> property,<br/>
    /// - [ProductName] => <see cref="AssemblyProductAttribute.Product"/> property.<br/>
    /// When the assembly attributes are not set, following fallback paths are used:<br/>
    /// - [LocalAppDataFolder]\[ProductName] - when the Company attribute is not set or is equal to Product attribute value,<br/>
    /// - [LocalAppDataFolder]\[ApplicationName] - when no assembly attributes are set.<br/>
    /// </remarks>
    public static string LocalAppDataTarget {
        get {
            if (_LocalAppDataTarget is not null) return _LocalAppDataTarget;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var executable = Assembly.GetEntryAssembly()!;
            var company = executable.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            var product = executable.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            return _LocalAppDataTarget =
                company is not null && product is not null && company != product ? SysPath.Combine(localAppData, company, product) :
                product is not null ? SysPath.Combine(localAppData, product) :
                SysPath.Combine(localAppData, Application.Name);
        }
    }

    /// <summary>
    /// Gets the home directory target like "~/.[app]" for the curren Linux user.
    /// </summary>
    public static string HomeDirectoryTarget
        => _UserHomeDirectoryTarget ??=
            SysPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), '.' + Application.Name);

    /// <summary>
    /// Gets the target directory for user files, platform dependent.<br/>
    /// For Windows it will be located in %LOCALAPPDATA%, for Linux - in ~.<br/>
    /// See <see cref="LocalAppDataTarget"/> and <see cref="HomeDirectoryTarget"/> documentation.
    /// </summary>
    public static string UserTarget =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? LocalAppDataTarget :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? HomeDirectoryTarget :
        throw new PlatformNotSupportedException();

    /// <summary>
    /// Locates a configuration file for the specified configuration name.
    /// </summary>
    /// <param name="name">Configuration name, or null for default name matching the main executable name.</param>
    /// <returns>A full path to the file, or null if the file is not found.</returns>
    public string Locate(string? name) => FilePath = Locate(name, create: true, preferUserDirectory: false);

    /// <summary>
    /// Locates a configuration file for the specified configuration name.
    /// </summary>
    /// <param name="name">Configuration name, or null for default name matching the main executable name.</param>
    /// <param name="create">Create if doesn't exitst or throw if doesn't exist.</param>
    /// <param name="preferUserDirectory">Set to search the user directory first.</param>
    /// <returns>A full path to the file, or null if the file is not found.</returns>
    public virtual string Locate(string? name, bool create, bool preferUserDirectory) {
        var programDirectory = Application.Directory;
        var userDirectory = UserTarget;
        var targets = preferUserDirectory
            ? new[] { userDirectory, programDirectory }
            : new[] { programDirectory, userDirectory };
        IEnumerable<string> getDebugExtensions() { // for debug configuration ".dev" files should be matched first!
            foreach (var extension in Extensions) {
                yield return ".dev" + extension;
                yield return extension;
            }
        }
        var extensions = IsDebug ? getDebugExtensions() : Extensions;
        string? path = null;
        foreach (var target in targets) {
            foreach (var extension in extensions) {
                path = SysPath.Combine(target, name + extension);
                if (File.Exists(path)) return path;
            }
        }
        if (create) {
            var extension = Extensions.First();
            path = SysPath.Combine(userDirectory, name + extension);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
            var blankConfigData = new byte[] { (byte)'{', (byte)'}' };
            stream.Write(blankConfigData.AsSpan());
            stream.Close();
            return path;
        }
        throw new FileNotFoundException("Configuration file not found", path);
    }

    /// <summary>
    /// <see cref="LocalAppDataTarget"/> backing field.
    /// </summary>
    private static string? _LocalAppDataTarget;

    /// <summary>
    /// <see cref="HomeDirectoryTarget"/> backing field.
    /// </summary>
    private static string? _UserHomeDirectoryTarget;

}
