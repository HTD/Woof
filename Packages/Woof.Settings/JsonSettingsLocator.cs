namespace Woof.Settings;

/// <summary>
/// Locates the configuration files.
/// </summary>
public class JsonSettingsLocator : ILocator {

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
    /// Gets or sets a value indicating that the user directory will be preferred over the application directory.
    /// </summary>
    public static bool PreferUserDirectory { get; set; }

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
            var executable = Executable.Assembly;
            var company = executable.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            var product = executable.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            return _LocalAppDataTarget =
                company is not null && product is not null && company != product ? Path.Combine(localAppData, company, product) :
                product is not null ? Path.Combine(localAppData, product) :
                Path.Combine(localAppData, Executable.FileName);
        }
    }

    /// <summary>
    /// Gets the home directory target like "~/.[app]" for the curren Linux user.
    /// </summary>
    public static string HomeDirectoryTarget
        => _UserHomeDirectoryTarget ??=
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), '.' + Executable.FileName);

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
    /// Locates a settings file for the specified settings name, or it uses application name if the name is not specified.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>A tuple with the full path to the file and a flag indicating whether the file exists.</returns>
    public virtual (string path, bool exists) Locate(string name) {
        if (name is null) throw new ArgumentNullException(nameof(name));
        var programDirectory = Executable.Directory.FullName;
        var userDirectory = UserTarget;
        var targets =
            PreferUserDirectory
            ? new[] { userDirectory, programDirectory }
            : new[] { programDirectory, userDirectory };
        var extensions = IsDebug ? GetDebugExtensions() : Extensions;
        string? path = null;
        foreach (var target in targets) {
            foreach (var extension in extensions) {
                path = Path.Combine(target, name + extension);
                if (File.Exists(path)) return (path, true);
            }
        }
        {
            var extension = Extensions.First();
            if (!Directory.Exists(userDirectory)) Directory.CreateDirectory(userDirectory);
            return (Path.Combine(userDirectory, name + extension), false);
        }
    }

    /// <summary>
    /// Finds the primary and secondary location for the new settings file. One of these should be used to save a new file.
    /// </summary>
    /// <param name="name">Settings file base name (no extension).</param>
    /// <returns>
    /// A tuple containing the primary and secondary paths.
    /// The caller can decide whether to use the secondary path if the primary path is not writeable.
    /// </returns>
    public virtual (string primaryPath, string secondaryPath) LocateNew(string name) {
        if (name is null) throw new ArgumentNullException(nameof(name));
        var programDirectory = Executable.Directory.FullName;
        var userDirectory = UserTarget;
        var extension = Extensions.First();
        return (
            Path.Combine(PreferUserDirectory ? userDirectory : programDirectory, name + extension),
            Path.Combine(PreferUserDirectory ? programDirectory : userDirectory, name + extension)
        );
    }

    /// <summary>
    /// Gets the extensions for the debug build.
    /// </summary>
    /// <returns>Extensions enumeration with the debug extensions placed first.</returns>
    private IEnumerable<string> GetDebugExtensions() {
        foreach (var extension in Extensions) {
            yield return ".dev" + extension;
            yield return extension;
        }
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
